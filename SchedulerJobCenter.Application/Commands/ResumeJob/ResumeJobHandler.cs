using MediatR;
using SchedulerJobCenter.Domain.Enums;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Infrastructure.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.Commands.ResumeJob
{
    public class ResumeJobHandler(IJobRepository jobRepo, ISchedulerService schedulerService) : IRequestHandler<ResumeJobCommand, bool>
    {
        public async Task<bool> Handle(ResumeJobCommand cmd, CancellationToken ct)
        {
            var job = await jobRepo.GetByIdAsync(cmd.Id, ct);
            if (job is null || job.Status == JobStatus.Deleted) return false;

            job.Status = JobStatus.Active;
            job.UpdatedAt = DateTime.UtcNow;

            await jobRepo.SaveChangesAsync(ct);
            await schedulerService.ResumeJobAsync(cmd.Id, ct);

            return true;
        }
    }
}
