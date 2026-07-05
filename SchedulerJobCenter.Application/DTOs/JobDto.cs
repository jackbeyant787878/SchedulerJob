
using SchedulerJobCenter.Domain.Entities;
using SchedulerJobCenter.Domain.Enums;

namespace SchedulerJobCenter.Application.DTOs
{
    public class JobDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string CronExpression { get; init; } = string.Empty;
        public string TimeZoneId { get; init; } = string.Empty;
        public string TargetUrl { get; init; } = string.Empty;
        public string HttpMethod { get; init; } = string.Empty;
        public string? RequestHeaders { get; init; }
        public string? RequestBody { get; init; }
        public int TimeoutSeconds { get; init; }
        public int MaxRetryCount { get; init; }
        public JobStatus Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        public static JobDto From(ScheduledJob job) => new()
        {
            Id = job.Id,
            Name = job.Name,
            Description = job.Description,
            CronExpression = job.CronExpression,
            TimeZoneId = job.TimeZoneId,
            TargetUrl = job.TargetUrl,
            HttpMethod = job.HttpMethod,
            RequestHeaders = job.RequestHeaders,
            RequestBody = job.RequestBody,
            TimeoutSeconds = job.TimeoutSeconds,
            MaxRetryCount = job.MaxRetryCount,
            Status = job.Status,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt
        };
    }
}
