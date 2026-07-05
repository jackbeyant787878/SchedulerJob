using Microsoft.Extensions.Logging;
using Quartz;
using SchedulerJobCenter.Domain.Entities;
namespace SchedulerJobCenter.Infrastructure.Scheduling
{
    public class SchedulerService(ISchedulerFactory schedulerFactory,ILogger<SchedulerService> logger) : ISchedulerService
    {
        private IScheduler? _scheduler;
        private async Task<IScheduler> GetSchedulerAsync(CancellationToken ct = default)
        {
            _scheduler ??= await schedulerFactory.GetScheduler(ct);
            return _scheduler;
        }

        public async Task ScheduleJobAsync(ScheduledJob job, CancellationToken ct = default)
        {
            var scheduler = await GetSchedulerAsync(ct);

            var jobKey = BuildJobKey(job.Id);
            var triggerKey = BuildTriggerKey(job.Id);

            var timeZone = GetTimeZone(job.TimeZoneId);

            var jobDetail = JobBuilder.Create<HttpCallJob>()
                .WithIdentity(jobKey)
                .UsingJobData(HttpCallJob.JobIdKey, job.Id.ToString())
                .StoreDurably(false)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .ForJob(jobKey)
                .WithCronSchedule(job.CronExpression, c => c
                    .InTimeZone(timeZone)
                    .WithMisfireHandlingInstructionDoNothing()) // Skip execution for missed triggers
                .Build();

            // Replace if the job already exists
            if (await scheduler.CheckExists(jobKey, ct))
            {
                await scheduler.DeleteJob(jobKey, ct);
                logger.LogInformation("Existing Quartz job replaced: {JobKey}", jobKey);
            }

            await scheduler.ScheduleJob(jobDetail, trigger, ct);
            logger.LogInformation("Job scheduled: {JobKey} Cron={Cron} TimeZone={TZ}",
                jobKey, job.CronExpression, job.TimeZoneId);
        }

        public async Task RescheduleJobAsync(ScheduledJob job, CancellationToken ct = default)
        {
            // Reuse ScheduleJobAsync directly, replacement logic is handled internally
            await ScheduleJobAsync(job, ct);
        }

        public async Task DeleteJobAsync(Guid jobId, CancellationToken ct = default)
        {
            var scheduler = await GetSchedulerAsync(ct);
            var jobKey = BuildJobKey(jobId);

            if (await scheduler.CheckExists(jobKey, ct))
            {
                await scheduler.DeleteJob(jobKey, ct);
                logger.LogInformation("Quartz job deleted: {JobKey}", jobKey);
            }
        }

        public async Task PauseJobAsync(Guid jobId, CancellationToken ct = default)
        {
            var scheduler = await GetSchedulerAsync(ct);
            await scheduler.PauseJob(BuildJobKey(jobId), ct);
            logger.LogInformation("Quartz job paused: {JobId}", jobId);
        }

        public async Task ResumeJobAsync(Guid jobId, CancellationToken ct = default)
        {
            var scheduler = await GetSchedulerAsync(ct);
            await scheduler.ResumeJob(BuildJobKey(jobId), ct);
            logger.LogInformation("Quartz job resumed: {JobId}", jobId);
        }

        public async Task TriggerNowAsync(Guid jobId, CancellationToken ct = default)
        {
            var scheduler = await GetSchedulerAsync(ct);
            var jobKey = BuildJobKey(jobId);

            if (!await scheduler.CheckExists(jobKey, ct))
            {
                logger.LogWarning("TriggerNow: Job {JobId} not found in scheduler.", jobId);
                return;
            }

            await scheduler.TriggerJob(jobKey, ct);
            logger.LogInformation("Quartz job manually triggered: {JobId}", jobId);
        }

        /// <summary>
        /// Load all active jobs from database and register them to Quartz on application startup.
        /// </summary>
        public async Task LoadAllJobsAsync(CancellationToken ct = default)
        {
            var scheduler = await GetSchedulerAsync(ct);
            if (!scheduler.IsStarted) await scheduler.Start(ct);

            logger.LogInformation("Quartz scheduler started, loading jobs from DB...");
        }

        // ── Private Helpers ──────────────────────────────────────────

        private static JobKey BuildJobKey(Guid jobId) =>
            new(jobId.ToString(), HttpCallJob.GroupName);

        private static TriggerKey BuildTriggerKey(Guid jobId) =>
            new($"trigger-{jobId}", HttpCallJob.GroupName);

        private static TimeZoneInfo GetTimeZone(string timeZoneId)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
            catch { return TimeZoneInfo.Utc; }
        }
    }
}
