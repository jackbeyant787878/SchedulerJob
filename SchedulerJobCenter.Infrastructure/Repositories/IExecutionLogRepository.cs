using SchedulerJobCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Infrastructure.Repositories
{
    public interface IExecutionLogRepository
    {
        Task<(IEnumerable<ExecutionLog> Items, int Total)> GetPagedByJobAsync(
            Guid? jobId, int page, int pageSize, CancellationToken ct = default);
        Task AddAsync(ExecutionLog log, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        /// <summary>Clean up logs older than the specified number of days</summary>
        Task<int> CleanupOldLogsAsync(int keepDays, CancellationToken ct = default);
    }
}
