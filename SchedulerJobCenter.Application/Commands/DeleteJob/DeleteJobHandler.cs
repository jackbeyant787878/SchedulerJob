using MediatR;
using SchedulerJobCenter.Domain.Enums;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Infrastructure.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.Commands.DeleteJob
{
    public class DeleteJobHandler(IJobRepository jobRepo, ISchedulerService schedulerService) : IRequestHandler<DeleteJobCommand, bool>
    {
        public async Task<bool> Handle(DeleteJobCommand cmd, CancellationToken ct)
        {
            var job = await jobRepo.GetByIdAsync(cmd.Id, ct);
            if (job is null) return false;

            // soft delete
            job.Status = JobStatus.Deleted;
            job.UpdatedAt = DateTime.UtcNow;

            await jobRepo.SaveChangesAsync(ct);
            await schedulerService.DeleteJobAsync(cmd.Id, ct);

            return true;
        }
    }
}
