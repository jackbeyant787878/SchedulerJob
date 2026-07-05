using MediatR;
using SchedulerJobCenter.Domain.Enums;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Infrastructure.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.Commands.PauseJob
{
    public class PauseJobHandler(IJobRepository jobRepo,ISchedulerService schedulerService) : IRequestHandler<PauseJobCommand, bool>
    {
        public async Task<bool> Handle(PauseJobCommand cmd, CancellationToken ct)
        {
            var job = await jobRepo.GetByIdAsync(cmd.Id, ct);
            if (job is null || job.Status == JobStatus.Deleted) return false;

            job.Status = JobStatus.Paused;
            job.UpdatedAt = DateTime.UtcNow;

            await jobRepo.SaveChangesAsync(ct);
            await schedulerService.PauseJobAsync(cmd.Id, ct);

            return true;
        }
    }
}
