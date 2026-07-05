using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.Services
{
    public interface IJobService
    {
        Task<PagedResult<JobDto>> GetJobsAsync(int page, int pageSize, string? keyword, CancellationToken ct = default);
        Task<JobDto?> GetJobByIdAsync(Guid id, CancellationToken ct = default);
        Task<JobDto> CreateJobAsync(CreateJobRequest request, CancellationToken ct = default);
        Task<JobDto?> UpdateJobAsync(Guid id, UpdateJobRequest request, CancellationToken ct = default);
        Task<bool> DeleteJobAsync(Guid id, CancellationToken ct = default);
        Task<bool> PauseJobAsync(Guid id, CancellationToken ct = default);
        Task<bool> ResumeJobAsync(Guid id, CancellationToken ct = default);
        Task<bool> TriggerJobNowAsync(Guid id, CancellationToken ct = default);
    }
}
