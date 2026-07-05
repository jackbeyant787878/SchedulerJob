using System.ComponentModel.DataAnnotations;
namespace SchedulerJobCenter.Application.DTOs
{
    public class CreateJobRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Quartz Cron expression (6-digit with seconds: Second Minute Hour Day Month Week)
        /// Example: 0 0/5 * * * ? means execute every 5 minutes
        /// </summary>
        [Required]
        public string CronExpression { get; set; } = string.Empty;

        [Required]
        [Url]
        public string TargetUrl { get; set; } = string.Empty;

        [RegularExpression("^(GET|POST)$", ErrorMessage = "HttpMethod must be GET or POST")]
        public string HttpMethod { get; set; } = "GET";

        public string? RequestBody { get; set; }

        /// <summary>JSON format, e.g.: {"Authorization":"Bearer xxx"}</summary>
        public string? RequestHeaders { get; set; }

        /// <summary>IANA time zone ID, e.g.: Asia/Shanghai, America/New_York, UTC</summary>
        [Required]
        public string TimeZoneId { get; set; } = "UTC";

        [Range(5, 300)]
        public int TimeoutSeconds { get; set; } = 30;

        [Range(0, 5)]
        public int MaxRetryCount { get; set; } = 0;
    }
}
