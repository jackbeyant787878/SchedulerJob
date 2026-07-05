using Microsoft.EntityFrameworkCore;
using SchedulerJobCenter.Domain.Entities;
namespace SchedulerJobCenter.Infrastructure.Data
{
    public class SchedulerJobDbContext(DbContextOptions<SchedulerJobDbContext> options) : DbContext(options)
    {
        public DbSet<ScheduledJob> ScheduledJobs => Set<ScheduledJob>();
        public DbSet<ExecutionLog> ExecutionLogs => Set<ExecutionLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ScheduledJob>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.CronExpression).HasMaxLength(200).IsRequired();
                e.Property(x => x.TargetUrl).HasMaxLength(2000).IsRequired();
                e.Property(x => x.HttpMethod).HasMaxLength(10).IsRequired();
                e.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
                e.Property(x => x.RequestBody).HasMaxLength(10000);
                e.Property(x => x.RequestHeaders).HasMaxLength(2000);
                e.Property(x => x.Description).HasMaxLength(1000);
            });

            modelBuilder.Entity<ExecutionLog>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.JobId);
                e.HasIndex(x => x.FiredAt);
                e.Property(x => x.ResponseBody).HasMaxLength(4000);
                e.Property(x => x.ErrorMessage).HasMaxLength(4000);

                e.HasOne(x => x.Job)
                 .WithMany(x => x.ExecutionLogs)
                 .HasForeignKey(x => x.JobId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
