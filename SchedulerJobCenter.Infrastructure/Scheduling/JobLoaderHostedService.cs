using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchedulerJobCenter.Infrastructure.Repositories;

namespace SchedulerJobCenter.Infrastructure.Scheduling
{
    /// <summary>
    /// Register all active jobs from database to Quartz when the application starts,
    /// ensuring schedules are retained after service restarts.
    /// </summary>
    public class JobLoaderHostedService(IServiceScopeFactory scopeFactory, ISchedulerService schedulerService,ILogger<JobLoaderHostedService> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken ct)
        {
            logger.LogInformation("JobLoaderHostedService: Loading active jobs...");

            await using var scope = scopeFactory.CreateAsyncScope();
            var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            var activeJobs = await jobRepo.GetActiveJobsAsync(ct);
            var count = 0;

            foreach (var job in activeJobs)
            {
                try
                {
                    await schedulerService.ScheduleJobAsync(job, ct);
                    count++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to schedule job {JobId} on startup.", job.Id);
                }
            }

            logger.LogInformation("JobLoaderHostedService: {Count} jobs loaded into Quartz.", count);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
