using System.ComponentModel.DataAnnotations;
namespace SchedulerJobCenter.Application.DTOs
{
    public class UpdateJobRequest
    {
        [StringLength(200, MinimumLength = 1)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? CronExpression { get; set; }

        [Url]
        public string? TargetUrl { get; set; }

        [RegularExpression("^(GET|POST)$")]
        public string? HttpMethod { get; set; }

        public string? RequestBody { get; set; }
        public string? RequestHeaders { get; set; }

        public string? TimeZoneId { get; set; }

        [Range(5, 300)]
        public int? TimeoutSeconds { get; set; }

        [Range(0, 5)]
        public int? MaxRetryCount { get; set; }
    }
}
