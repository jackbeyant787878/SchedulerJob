using MediatR;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.Queries.GetExecutionLogs
{
    public record GetExecutionLogsQuery(Guid? JobId,int Page,int PageSize) : IRequest<PagedResult<ExecutionLogDto>>;
}
