using MediatR;
using SchedulerJobCenter.Domain.Enums;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Infrastructure.Scheduling;
namespace SchedulerJobCenter.Application.Commands.TriggerJob
{
    public class TriggerJobHandler(IJobRepository jobRepo,ISchedulerService schedulerService) : IRequestHandler<TriggerJobCommand, bool>
    {
        public async Task<bool> Handle(TriggerJobCommand cmd, CancellationToken ct)
        {
            var job = await jobRepo.GetByIdAsync(cmd.Id, ct);
            if (job is null || job.Status == JobStatus.Deleted) return false;

            await schedulerService.TriggerNowAsync(cmd.Id, ct);
            return true;
        }
    }
}
