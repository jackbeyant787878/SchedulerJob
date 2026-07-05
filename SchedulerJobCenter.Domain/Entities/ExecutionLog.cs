using SchedulerJobCenter.Domain.Enums;
namespace SchedulerJobCenter.Domain.Entities
{
    public class ExecutionLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public ScheduledJob Job { get; set; } = null!;

        public ExecutionStatus Status { get; set; }

        /// <summary>Trigger time (UTC)</summary>
        public DateTime FiredAt { get; set; }

        /// <summary>Completion time (UTC)</summary>
        public DateTime? FinishedAt { get; set; }

        /// <summary>Execution duration in milliseconds</summary>
        public long? ElapsedMs { get; set; }

        /// <summary>HTTP response status code</summary>
        public int? HttpStatusCode { get; set; }

        /// <summary>Response body (truncated to first 4000 characters)</summary>
        public string? ResponseBody { get; set; }

        /// <summary>Exception details</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Retry sequence number (0 = initial execution)</summary>
        public int RetryCount { get; set; } = 0;
    }
}
