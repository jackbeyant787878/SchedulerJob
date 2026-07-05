using SchedulerJobCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Infrastructure.Repositories
{
    public interface IJobRepository
    {
        Task<(IEnumerable<ScheduledJob> Items, int Total)> GetPagedAsync(int page, int pageSize, string? keyword, CancellationToken ct = default);
        Task<ScheduledJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<ScheduledJob>> GetActiveJobsAsync(CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default);
        Task AddAsync(ScheduledJob job, CancellationToken ct = default);
        void Update(ScheduledJob job);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
