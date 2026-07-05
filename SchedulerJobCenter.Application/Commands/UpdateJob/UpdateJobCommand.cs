using MediatR;
using SchedulerJobCenter.Application.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SchedulerJobCenter.Application.Commands.UpdateJob
{
    public record UpdateJobCommand : IRequest<JobDto>
    {
        public Guid Id { get; init; }

        [MaxLength(100)]
        public string? Name { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }

        public string? CronExpression { get; init; }

        public string? TimeZoneId { get; init; }

        [Url]
        public string? TargetUrl { get; init; }

        public string? HttpMethod { get; init; }

        public string? RequestHeaders { get; init; }

        public string? RequestBody { get; init; }

        [Range(1, 300)]
        public int? TimeoutSeconds { get; init; }

        [Range(0, 5)]
        public int? MaxRetryCount { get; init; }
    }
}
