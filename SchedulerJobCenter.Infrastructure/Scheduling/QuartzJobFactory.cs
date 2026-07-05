using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
namespace SchedulerJobCenter.Infrastructure.Scheduling
{
    /// <summary>
    /// Allows Quartz to instantiate Job instances via DI container and support constructor injection.
    /// </summary>
    public class QuartzJobFactory(IServiceProvider serviceProvider) : IJobFactory
    {
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobType = bundle.JobDetail.JobType;
            return (IJob)serviceProvider.GetRequiredService(jobType);
        }

        public void ReturnJob(IJob job)
        {
            // Lifecycle managed by DI; no manual release required here
            if (job is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
