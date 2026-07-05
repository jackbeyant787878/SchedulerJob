using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Application.Queries.GetExecutionLogs;
using SchedulerJobCenter.Shared;

namespace SchedulerJobCenter.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ExecutionLogsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] Guid? jobId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 20;

            var result = await mediator.Send(new GetExecutionLogsQuery(jobId, page, pageSize), ct);
            return Ok(ApiResponse<PagedResult<ExecutionLogDto>>.Ok(result));
        }
    }
}
