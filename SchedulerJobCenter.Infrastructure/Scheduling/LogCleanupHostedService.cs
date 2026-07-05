using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchedulerJobCenter.Infrastructure.Configuration;
using SchedulerJobCenter.Infrastructure.Repositories;
namespace SchedulerJobCenter.Infrastructure.Scheduling
{
    /// <summary>
    /// Clean up execution logs exceeding retention days every early morning to prevent unbounded database growth.
    /// </summary>
    public class LogCleanupHostedService( IServiceScopeFactory scopeFactory,IOptions<SchedulerJobOptions> options, ILogger<LogCleanupHostedService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // Run once per day
                await Task.Delay(TimeSpan.FromHours(24), ct);

                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var logRepo = scope.ServiceProvider.GetRequiredService<IExecutionLogRepository>();

                    var keepDays = options.Value.LogRetentionDays;
                    var deleted = await logRepo.CleanupOldLogsAsync(keepDays, ct);
                    logger.LogInformation("LogCleanup: Deleted {Count} logs older than {Days} days.", deleted, keepDays);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "LogCleanup: Unexpected error during cleanup.");
                }
            }
        }
    }
}
