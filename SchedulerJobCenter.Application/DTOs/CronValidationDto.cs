using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.DTOs
{
    public class CronValidationDto
    {
        public bool IsValid { get; init; }
        public List<string> NextFireTimes { get; init; } = [];
        public string TimeZoneId { get; init; } = string.Empty;
    }
}
