using MediatR;
using SchedulerJobCenter.Application.DTOs;
using System.ComponentModel.DataAnnotations;
namespace SchedulerJobCenter.Application.Commands.CreateJob
{
    public record CreateJobCommand : IRequest<JobDto>
    {
        [Required, MaxLength(100)]
        public string Name { get; init; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; init; }

        [Required]
        public string CronExpression { get; init; } = string.Empty;

        [Required]
        public string TimeZoneId { get; init; } = "UTC";

        [Required, Url]
        public string TargetUrl { get; init; } = string.Empty;

        public string HttpMethod { get; init; } = "GET";

        public string? RequestHeaders { get; init; }

        public string? RequestBody { get; init; }

        [Range(1, 300)]
        public int TimeoutSeconds { get; init; } = 30;

        [Range(0, 5)]
        public int MaxRetryCount { get; init; } = 0;
    }
}
