using SchedulerJobCenter.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Domain.Entities
{
    public class ScheduledJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Unique global task name</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Task description</summary>
        public string? Description { get; set; }

        /// <summary>Cron expression (Quartz format, 6-digit second-level)</summary>
        public string CronExpression { get; set; } = string.Empty;

        /// <summary>Target URL, HTTP endpoint to invoke during scheduling</summary>
        public string TargetUrl { get; set; } = string.Empty;

        /// <summary>HTTP Method: GET / POST</summary>
        public string HttpMethod { get; set; } = "GET";

        /// <summary>Request body sent when using POST method</summary>
        public string? RequestBody { get; set; }

        /// <summary>Custom request headers, stored as serialized JSON</summary>
        public string? RequestHeaders { get; set; }

        /// <summary>IANA time zone ID, e.g. Asia/Shanghai</summary>
        public string TimeZoneId { get; set; } = "UTC";

        /// <summary>HTTP request timeout in seconds</summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>Maximum retry attempts upon failure</summary>
        public int MaxRetryCount { get; set; } = 0;

        public JobStatus Status { get; set; } = JobStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ExecutionLog> ExecutionLogs { get; set; } = [];
    }
}
