using MediatR;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Shared;
namespace SchedulerJobCenter.Application.Queries.GetJobs
{
    public record GetJobsQuery(int Page, int PageSize,string? Keyword) : IRequest<PagedResult<JobDto>>;
}
