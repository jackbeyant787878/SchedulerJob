using Microsoft.Extensions.Logging;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Domain.Entities;
using SchedulerJobCenter.Domain.Enums;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Infrastructure.Scheduling;
using SchedulerJobCenter.Shared;
namespace SchedulerJobCenter.Application.Services
{
    public class JobService(IJobRepository jobRepo, ISchedulerService schedulerService, ILogger<JobService> logger) : IJobService
    {
        public async Task<PagedResult<JobDto>> GetJobsAsync(
            int page, int pageSize, string? keyword, CancellationToken ct = default)
        {
            var (items, total) = await jobRepo.GetPagedAsync(page, pageSize, keyword, ct);
            return new PagedResult<JobDto>
            {
                Items = items.Select(MapToDto),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<JobDto?> GetJobByIdAsync(Guid id, CancellationToken ct = default)
        {
            var job = await jobRepo.GetByIdAsync(id, ct);
            return job is null ? null : MapToDto(job);
        }

        public async Task<JobDto> CreateJobAsync(CreateJobRequest request, CancellationToken ct = default)
        {
            // 验证时区
            ValidateTimeZone(request.TimeZoneId);

            // 验证 Cron 表达式
            ValidateCron(request.CronExpression);

            // 检查名称唯一性
            if (await jobRepo.ExistsByNameAsync(request.Name, null, ct))
                throw new InvalidOperationException($"Job name '{request.Name}' already exists.");

            var job = new ScheduledJob
            {
                Name = request.Name,
                Description = request.Description,
                CronExpression = request.CronExpression,
                TargetUrl = request.TargetUrl,
                HttpMethod = request.HttpMethod.ToUpperInvariant(),
                RequestBody = request.RequestBody,
                RequestHeaders = request.RequestHeaders,
                TimeZoneId = request.TimeZoneId,
                TimeoutSeconds = request.TimeoutSeconds,
                MaxRetryCount = request.MaxRetryCount,
                Status = JobStatus.Active
            };

            await jobRepo.AddAsync(job, ct);
            await jobRepo.SaveChangesAsync(ct);

            // 注册到 Quartz
            await schedulerService.ScheduleJobAsync(job, ct);

            logger.LogInformation("Job created and scheduled: {JobId} {JobName}", job.Id, job.Name);
            return MapToDto(job);
        }

        public async Task<JobDto?> UpdateJobAsync(Guid id, UpdateJobRequest request, CancellationToken ct = default)
        {
            var job = await jobRepo.GetByIdAsync(id, ct);
            if (job is null) return null;

            if (request.TimeZoneId is not null) ValidateTimeZone(request.TimeZoneId);
            if (request.CronExpression is not null) ValidateCron(request.CronExpression);

            if (request.Name is not null)
            {
                if (await jobRepo.ExistsByNameAsync(request.Name, id, ct))
                    throw new InvalidOperationException($"Job name '{request.Name}' already exists.");
                job.Name = request.Name;
            }

            if (request.Description is not null) job.Description = request.Description;
            if (request.CronExpression is not null) job.CronExpression = request.CronExpression;
            if (request.TargetUrl is not null) job.TargetUrl = request.TargetUrl;
            if (request.HttpMethod is not null) job.HttpMethod = request.HttpMethod.ToUpperInvariant();
            if (request.RequestBody is not null) job.RequestBody = request.RequestBody;
            if (request.RequestHeaders is not null) job.RequestHeaders = request.RequestHeaders;
            if (request.TimeZoneId is not null) job.TimeZoneId = request.TimeZoneId;
            if (request.TimeoutSeconds.HasValue) job.TimeoutSeconds = request.TimeoutSeconds.Value;
            if (request.MaxRetryCount.HasValue) job.MaxRetryCount = request.MaxRetryCount.Value;
            job.UpdatedAt = DateTime.UtcNow;

            jobRepo.Update(job);
            await jobRepo.SaveChangesAsync(ct);

            // 重新调度（删除旧触发器，注册新的）
            await schedulerService.RescheduleJobAsync(job, ct);

            logger.LogInformation("Job updated and rescheduled: {JobId}", id);
            return MapToDto(job);
        }

        public async Task<bool> DeleteJobAsync(Guid id, CancellationToken ct = default)
        {
            var job = await jobRepo.GetByIdAsync(id, ct);
            if (job is null) return false;

            job.Status = JobStatus.Deleted;
            job.UpdatedAt = DateTime.UtcNow;
            jobRepo.Update(job);
            await jobRepo.SaveChangesAsync(ct);

            await schedulerService.DeleteJobAsync(id, ct);

            logger.LogInformation("Job deleted: {JobId}", id);
            return true;
        }

        public async Task<bool> PauseJobAsync(Guid id, CancellationToken ct = default)
        {
            var job = await jobRepo.GetByIdAsync(id, ct);
            if (job is null || job.Status == JobStatus.Deleted) return false;

            job.Status = JobStatus.Paused;
            job.UpdatedAt = DateTime.UtcNow;
            jobRepo.Update(job);
            await jobRepo.SaveChangesAsync(ct);

            await schedulerService.PauseJobAsync(id, ct);
            logger.LogInformation("Job paused: {JobId}", id);
            return true;
        }

        public async Task<bool> ResumeJobAsync(Guid id, CancellationToken ct = default)
        {
            var job = await jobRepo.GetByIdAsync(id, ct);
            if (job is null || job.Status == JobStatus.Deleted) return false;

            job.Status = JobStatus.Active;
            job.UpdatedAt = DateTime.UtcNow;
            jobRepo.Update(job);
            await jobRepo.SaveChangesAsync(ct);

            await schedulerService.ResumeJobAsync(id, ct);
            logger.LogInformation("Job resumed: {JobId}", id);
            return true;
        }

        public async Task<bool> TriggerJobNowAsync(Guid id, CancellationToken ct = default)
        {
            var job = await jobRepo.GetByIdAsync(id, ct);
            if (job is null || job.Status == JobStatus.Deleted) return false;

            await schedulerService.TriggerNowAsync(id, ct);
            logger.LogInformation("Job manually triggered: {JobId}", id);
            return true;
        }

        // ── 私有辅助 ──────────────────────────────────────────

        private static void ValidateTimeZone(string timeZoneId)
        {
            try { TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
            catch { throw new ArgumentException($"Invalid TimeZone ID: '{timeZoneId}'. Use IANA format, e.g. Asia/Shanghai"); }
        }

        private static void ValidateCron(string cron)
        {
            try { new Quartz.CronExpression(cron); }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid Cron expression '{cron}': {ex.Message}");
            }
        }

        private static JobDto MapToDto(ScheduledJob j) => new()
        {
            Id = j.Id,
            Name = j.Name,
            Description = j.Description,
            CronExpression = j.CronExpression,
            TargetUrl = j.TargetUrl,
            HttpMethod = j.HttpMethod,
            RequestBody = j.RequestBody,
            RequestHeaders = j.RequestHeaders,
            TimeZoneId = j.TimeZoneId,
            TimeoutSeconds = j.TimeoutSeconds,
            MaxRetryCount = j.MaxRetryCount,
            Status = j.Status,
            CreatedAt = j.CreatedAt,
            UpdatedAt = j.UpdatedAt
        };
    }
}
