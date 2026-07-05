using SchedulerJobCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Infrastructure.Scheduling
{
    public interface ISchedulerService
    {
        Task ScheduleJobAsync(ScheduledJob job, CancellationToken ct = default);
        Task RescheduleJobAsync(ScheduledJob job, CancellationToken ct = default);
        Task DeleteJobAsync(Guid jobId, CancellationToken ct = default);
        Task PauseJobAsync(Guid jobId, CancellationToken ct = default);
        Task ResumeJobAsync(Guid jobId, CancellationToken ct = default);
        Task TriggerNowAsync(Guid jobId, CancellationToken ct = default);
        Task LoadAllJobsAsync(CancellationToken ct = default);
    }
}
