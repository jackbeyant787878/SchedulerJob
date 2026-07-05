using SchedulerJobCenter.Domain.Entities;
using SchedulerJobCenter.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.DTOs
{
    public class ExecutionLogDto
    {
        public Guid Id { get; init; }
        public Guid JobId { get; init; }
        public string JobName { get; init; } = string.Empty;
        public ExecutionStatus Status { get; init; }
        public DateTime FiredAt { get; init; }
        public DateTime? FinishedAt { get; init; }
        public long? ElapsedMs { get; init; }
        public int? HttpStatusCode { get; init; }
        public string? ResponseBody { get; init; }
        public string? ErrorMessage { get; init; }
        public int RetryCount { get; init; }

        public static ExecutionLogDto From(ExecutionLog log) => new()
        {
            Id = log.Id,
            JobId = log.JobId,
            JobName = log.Job?.Name ?? string.Empty,
            Status = log.Status,
            FiredAt = log.FiredAt,
            FinishedAt = log.FinishedAt,
            ElapsedMs = log.ElapsedMs,
            HttpStatusCode = log.HttpStatusCode,
            ResponseBody = log.ResponseBody,
            ErrorMessage = log.ErrorMessage,
            RetryCount = log.RetryCount
        };
    }
}
