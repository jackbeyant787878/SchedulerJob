using MediatR;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Infrastructure.Repositories;
using SchedulerJobCenter.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.Queries.GetExecutionLogs
{
    public class GetExecutionLogsHandler(IExecutionLogRepository logRepo):IRequestHandler<GetExecutionLogsQuery, PagedResult<ExecutionLogDto>>
    {
        public async Task<PagedResult<ExecutionLogDto>> Handle(GetExecutionLogsQuery query, CancellationToken ct)
        {
            var (items, total) = await logRepo.GetPagedByJobAsync(query.JobId, query.Page, query.PageSize, ct);

            return new PagedResult<ExecutionLogDto>
            {
                Items = items.Select(ExecutionLogDto.From),
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }
}
