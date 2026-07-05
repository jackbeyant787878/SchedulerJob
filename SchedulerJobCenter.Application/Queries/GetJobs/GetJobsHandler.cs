using MediatR;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.Queries.GetJobs
{
    public class GetJobsHandler(IJobRepository jobRepo) : IRequestHandler<GetJobsQuery, PagedResult<JobDto>>
    {
        public async Task<PagedResult<JobDto>> Handle(GetJobsQuery query, CancellationToken ct)
        {
            var (items, total) = await jobRepo.GetPagedAsync(query.Page, query.PageSize, query.Keyword, ct);

            return new PagedResult<JobDto>
            {
                Items = items.Select(JobDto.From),
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }
}
