using MediatR;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Domain.Entities;
using SchedulerJobCenter.Domain.Enums;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Infrastructure.Scheduling;
namespace SchedulerJobCenter.Application.Commands.CreateJob
{
    public class CreateJobHandler( IJobRepository jobRepo,ISchedulerService schedulerService) : IRequestHandler<CreateJobCommand, JobDto>
    {
        public async Task<JobDto> Handle(CreateJobCommand cmd, CancellationToken ct)
        {
            // Validate Cron expression format
            ValidateCron(cmd.CronExpression);

            // Validate target time zone
            var tz = GetTimeZone(cmd.TimeZoneId);

            // Check job name uniqueness
            if (await jobRepo.ExistsByNameAsync(cmd.Name, null, ct))
                throw new InvalidOperationException($"Job with name '{cmd.Name}' already exists.");

            var job = new ScheduledJob
            {
                Id = Guid.NewGuid(),
                Name = cmd.Name,
                Description = cmd.Description,
                CronExpression = cmd.CronExpression,
                TimeZoneId = tz.Id,
                TargetUrl = cmd.TargetUrl,
                HttpMethod = cmd.HttpMethod.ToUpper(),
                RequestHeaders = cmd.RequestHeaders,
                RequestBody = cmd.RequestBody,
                TimeoutSeconds = cmd.TimeoutSeconds,
                MaxRetryCount = cmd.MaxRetryCount,
                Status = JobStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await jobRepo.AddAsync(job, ct);
            await jobRepo.SaveChangesAsync(ct);
            await schedulerService.ScheduleJobAsync(job, ct);

            return JobDto.From(job);
        }

        private static void ValidateCron(string cron)
        {
            try { _ = new Quartz.CronExpression(cron); }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid Cron expression: {ex.Message}");
            }
        }

        private static TimeZoneInfo GetTimeZone(string timeZoneId)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
            catch { throw new ArgumentException($"Invalid TimeZone: '{timeZoneId}'."); }
        }
    }
}
