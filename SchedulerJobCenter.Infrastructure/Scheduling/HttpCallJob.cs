using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SchedulerJobCenter.Domain.Entities;
using SchedulerJobCenter.Domain.Enums;
using SchedulerJobCenter.Infrastructure.Repositories;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SchedulerJobCenter.Infrastructure.Scheduling
{
    /// <summary>
    /// Quartz Job implementation: sends HTTP requests to target URL per configuration, records execution logs and supports retries.
    /// DisallowConcurrentExecution ensures the same job will not run concurrently.
    /// </summary>
    [DisallowConcurrentExecution]
    public class HttpCallJob(IServiceScopeFactory scopeFactory,ILogger<HttpCallJob> logger, IHttpClientFactory httpClientFactory) : IJob
    {
        public const string JobIdKey = "JobId";
        public const string GroupName = "HttpCallGroup";

        public async Task Execute(IJobExecutionContext context)
        {
            var jobIdStr = context.JobDetail.JobDataMap.GetString(JobIdKey);
            if (!Guid.TryParse(jobIdStr, out var jobId))
            {
                logger.LogError("HttpCallJob: Invalid JobId in JobDataMap: {Value}", jobIdStr);
                return;
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var logRepo = scope.ServiceProvider.GetRequiredService<IExecutionLogRepository>();

            var scheduledJob = await jobRepo.GetByIdAsync(jobId, context.CancellationToken);
            if (scheduledJob is null)
            {
                logger.LogWarning("HttpCallJob: ScheduledJob {JobId} not found, skipping.", jobId);
                return;
            }

            if (scheduledJob.Status != Domain.Enums.JobStatus.Active)
            {
                logger.LogInformation("HttpCallJob: Job {JobId} is not active, skipping.", jobId);
                return;
            }

            await ExecuteWithRetryAsync(scheduledJob, logRepo, context.CancellationToken);
        }

        private async Task ExecuteWithRetryAsync(
            ScheduledJob job,
            IExecutionLogRepository logRepo,
            CancellationToken ct)
        {
            var maxAttempts = job.MaxRetryCount + 1; // Initial run + retry times

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var log = new ExecutionLog
                {
                    JobId = job.Id,
                    FiredAt = DateTime.UtcNow,
                    Status = ExecutionStatus.Running,
                    RetryCount = attempt
                };

                await logRepo.AddAsync(log, ct);
                await logRepo.SaveChangesAsync(ct);

                var sw = Stopwatch.StartNew();
                try
                {
                    var (statusCode, responseBody) = await CallHttpAsync(job, ct);
                    sw.Stop();

                    log.FinishedAt = DateTime.UtcNow;
                    log.ElapsedMs = sw.ElapsedMilliseconds;
                    log.HttpStatusCode = statusCode;
                    log.ResponseBody = Truncate(responseBody, 4000);

                    if (statusCode >= 200 && statusCode < 300)
                    {
                        log.Status = ExecutionStatus.Success;
                        await logRepo.SaveChangesAsync(ct);

                        logger.LogInformation(
                            "Job {JobId} executed successfully. StatusCode={StatusCode} ElapsedMs={ElapsedMs} Attempt={Attempt}",
                            job.Id, statusCode, sw.ElapsedMilliseconds, attempt + 1);
                        return; // Exit retry loop on success
                    }

                    // Non-2xx HTTP status treated as failure
                    log.Status = attempt < job.MaxRetryCount ? ExecutionStatus.Failed : ExecutionStatus.Failed;
                    log.ErrorMessage = $"HTTP {statusCode}: Non-success status code.";
                    await logRepo.SaveChangesAsync(ct);

                    logger.LogWarning(
                        "Job {JobId} returned non-success HTTP {StatusCode}. Attempt={Attempt}/{Max}",
                        job.Id, statusCode, attempt + 1, maxAttempts);
                }
                catch (TaskCanceledException)
                {
                    sw.Stop();
                    log.FinishedAt = DateTime.UtcNow;
                    log.ElapsedMs = sw.ElapsedMilliseconds;
                    log.Status = ExecutionStatus.Timeout;
                    log.ErrorMessage = $"Request timed out after {job.TimeoutSeconds}s.";
                    await logRepo.SaveChangesAsync(ct);

                    logger.LogWarning("Job {JobId} timed out. Attempt={Attempt}/{Max}",
                        job.Id, attempt + 1, maxAttempts);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    log.FinishedAt = DateTime.UtcNow;
                    log.ElapsedMs = sw.ElapsedMilliseconds;
                    log.Status = ExecutionStatus.Failed;
                    log.ErrorMessage = Truncate(ex.ToString(), 4000);
                    await logRepo.SaveChangesAsync(ct);

                    logger.LogError(ex, "Job {JobId} threw exception. Attempt={Attempt}/{Max}",
                        job.Id, attempt + 1, maxAttempts);
                }

                // Wait before retry (exponential backoff: 1s, 2s, 4s...)
                if (attempt < job.MaxRetryCount)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    logger.LogInformation("Job {JobId} retrying in {Delay}s...", job.Id, delay.TotalSeconds);
                    await Task.Delay(delay, ct);
                }
            }
        }

        private async Task<(int StatusCode, string Body)> CallHttpAsync(ScheduledJob job, CancellationToken ct)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(job.TimeoutSeconds));

            var client = httpClientFactory.CreateClient("SchedulerJob");
            using var request = new HttpRequestMessage(job.HttpMethod == "POST" ? HttpMethod.Post : HttpMethod.Get,job.TargetUrl);

            // Inject custom request headers
            if (!string.IsNullOrWhiteSpace(job.RequestHeaders))
            {
                try
                {
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(job.RequestHeaders);
                    if (headers is not null)
                        foreach (var (key, value) in headers)
                            request.Headers.TryAddWithoutValidation(key, value);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Job {JobId}: Failed to parse RequestHeaders.", job.Id);
                }
            }

            // POST request body
            if (job.HttpMethod == "POST" && !string.IsNullOrWhiteSpace(job.RequestBody))
                request.Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            return ((int)response.StatusCode, body);
        }

        private static string? Truncate(string? value, int maxLength) =>value is null ? null :value.Length <= maxLength ? value :
            value[..maxLength];
    }
}
