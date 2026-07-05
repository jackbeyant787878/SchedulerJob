using Microsoft.EntityFrameworkCore;
using SchedulerJobCenter.Domain.Entities;
using SchedulerJobCenter.Domain.Enums;
using SchedulerJobCenter.Infrastructure.Data;
namespace SchedulerJobCenter.Infrastructure.Repositories
{

    public class JobRepository(SchedulerJobDbContext db) : IJobRepository
    {
        public async Task<(IEnumerable<ScheduledJob> Items, int Total)> GetPagedAsync(
            int page, int pageSize, string? keyword, CancellationToken ct = default)
        {
            var query = db.ScheduledJobs.Where(j => j.Status != JobStatus.Deleted) .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(j =>j.Name.ToLower().Contains(kw) || (j.Description != null && j.Description.ToLower().Contains(kw)));
            }

            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(j => j.CreatedAt).Skip((page - 1) * pageSize) .Take(pageSize).ToListAsync(ct);

            return (items, total);
        }

        public Task<ScheduledJob?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.ScheduledJobs.FirstOrDefaultAsync(j => j.Id == id, ct);

        public async Task<IEnumerable<ScheduledJob>> GetActiveJobsAsync(CancellationToken ct = default) =>
            await db.ScheduledJobs.Where(j => j.Status == JobStatus.Active).AsNoTracking() .ToListAsync(ct);

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default) =>
            await db.ScheduledJobs.AnyAsync(j =>j.Name == name && j.Status != JobStatus.Deleted &&(excludeId == null || j.Id != excludeId), ct);

        public async Task AddAsync(ScheduledJob job, CancellationToken ct = default) =>  await db.ScheduledJobs.AddAsync(job, ct);

        public void Update(ScheduledJob job) => db.ScheduledJobs.Update(job);

        public Task SaveChangesAsync(CancellationToken ct = default) =>db.SaveChangesAsync(ct);
    }
}
