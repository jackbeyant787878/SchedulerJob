using MediatR;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Infrastructure.Scheduling;
namespace SchedulerJobCenter.Application.Commands.UpdateJob
{
    public class UpdateJobHandler(IJobRepository jobRepo,ISchedulerService schedulerService) : IRequestHandler<UpdateJobCommand, JobDto>
    {
        public async Task<JobDto> Handle(UpdateJobCommand cmd, CancellationToken ct)
        {
            var job = await jobRepo.GetByIdAsync(cmd.Id, ct)
                ?? throw new KeyNotFoundException($"Job {cmd.Id} not found.");

            // Check name uniqueness if job name is modified
            if (cmd.Name is not null && cmd.Name != job.Name &&
                await jobRepo.ExistsByNameAsync(cmd.Name, job.Id, ct))
                throw new InvalidOperationException($"Job name '{cmd.Name}' already exists.");

            // Validate new Cron expression if provided
            if (cmd.CronExpression is not null)
            {
                try { _ = new Quartz.CronExpression(cmd.CronExpression); }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Invalid Cron expression: {ex.Message}");
                }
            }

            // Validate new time zone ID if provided
            if (cmd.TimeZoneId is not null)
            {
                try { TimeZoneInfo.FindSystemTimeZoneById(cmd.TimeZoneId); }
                catch { throw new ArgumentException($"Invalid TimeZone: '{cmd.TimeZoneId}'."); }
            }

            // Partial field update
            if (cmd.Name is not null) job.Name = cmd.Name;
            if (cmd.Description is not null) job.Description = cmd.Description;
            if (cmd.CronExpression is not null) job.CronExpression = cmd.CronExpression;
            if (cmd.TimeZoneId is not null) job.TimeZoneId = cmd.TimeZoneId;
            if (cmd.TargetUrl is not null) job.TargetUrl = cmd.TargetUrl;
            if (cmd.HttpMethod is not null) job.HttpMethod = cmd.HttpMethod.ToUpper();
            if (cmd.RequestHeaders is not null) job.RequestHeaders = cmd.RequestHeaders;
            if (cmd.RequestBody is not null) job.RequestBody = cmd.RequestBody;
            if (cmd.TimeoutSeconds is not null) job.TimeoutSeconds = cmd.TimeoutSeconds.Value;
            if (cmd.MaxRetryCount is not null) job.MaxRetryCount = cmd.MaxRetryCount.Value;
            job.UpdatedAt = DateTime.UtcNow;

            await jobRepo.SaveChangesAsync(ct);
            await schedulerService.RescheduleJobAsync(job, ct);

            return JobDto.From(job);
        }
    }
}
