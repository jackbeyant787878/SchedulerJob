using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SchedulerJobCenter.Infrastructure.Configuration;
using SchedulerJobCenter.Infrastructure.Data;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Infrastructure.Scheduling;
namespace SchedulerJobCenter.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            // ── Configuration Options ──────────────────────────────────────────
            services.Configure<SchedulerJobOptions>(configuration.GetSection(SchedulerJobOptions.SectionName));

            // ── EF Core (SQL Server) ─────────────────────────────
            services.AddDbContext<SchedulerJobDbContext>(opts =>
                opts.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null)));

            // ── Repository Layer ────────────────────────────────────────
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();

     
            // ── HTTP Client Factory ───────────────────────────────────────
            services.AddHttpClient("SchedulerJob", client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "SchedulerJob/1.0");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Allow self-signed certificates for internal microservice scenarios
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            // ── Quartz.NET Scheduler Configuration ────────────────────────────────────────
            services.AddQuartz(q =>
            {
                q.UseDefaultThreadPool(tp =>
                {
                    // Maximum concurrent threads, adjust based on machine CPU cores
                    tp.MaxConcurrency = 50;
                });

                // Create job instances via DI container
                q.UseJobFactory<QuartzJobFactory>();

                // In-memory storage for single-node deployment
                // Replace with UsePersistentStore + AdoJobStore for multi-node cluster
                q.UseInMemoryStore();
            });

            services.AddQuartzHostedService(opts =>
            {
                // Wait for running jobs to finish before shutdown
                opts.WaitForJobsToComplete = true;
                opts.AwaitApplicationStarted = true;
            });

            // ── Scheduling Core Services ───────────────────────────────
            services.AddSingleton<ISchedulerService, SchedulerService>();
            services.AddTransient<HttpCallJob>();

            // Load jobs from database on application startup
            services.AddHostedService<JobLoaderHostedService>();
            // Background service for periodic log cleanup
            services.AddHostedService<LogCleanupHostedService>();

            return services;
        }

    }
}
