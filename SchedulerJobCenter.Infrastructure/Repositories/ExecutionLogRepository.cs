using Microsoft.EntityFrameworkCore;
using SchedulerJobCenter.Domain.Entities;
using SchedulerJobCenter.Infrastructure.Data;
namespace SchedulerJobCenter.Infrastructure.Repositories
{
    public class ExecutionLogRepository(SchedulerJobDbContext db) : IExecutionLogRepository
    {
        public async Task<(IEnumerable<ExecutionLog> Items, int Total)> GetPagedByJobAsync(
            Guid? jobId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = db.ExecutionLogs.Include(l => l.Job) .AsNoTracking();

            if (jobId.HasValue) query = query.Where(l => l.JobId == jobId.Value);

            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(l => l.FiredAt) .Skip((page - 1) * pageSize).Take(pageSize) .ToListAsync(ct);
            return (items, total);
        }

        public async Task AddAsync(ExecutionLog log, CancellationToken ct = default) =>
            await db.ExecutionLogs.AddAsync(log, ct);

        public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

        public async Task<int> CleanupOldLogsAsync(int keepDays, CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-keepDays);
            return await db.ExecutionLogs.Where(l => l.FiredAt < cutoff).ExecuteDeleteAsync(ct);
        }
    }
}
