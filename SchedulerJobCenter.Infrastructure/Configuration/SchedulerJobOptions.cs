using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Infrastructure.Configuration
{

    public class SchedulerJobOptions
    {
        public const string SectionName = "SchedulerJob";

        /// <summary>Execution log retention days, default value is 30 days</summary>
        public int LogRetentionDays { get; set; } = 30;

        /// <summary>Maximum concurrent connections for HTTP client</summary>
        public int MaxHttpConnections { get; set; } = 100;
    }
}
