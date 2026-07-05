using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchedulerJobCenter.Application.Commands.CreateJob;
using SchedulerJobCenter.Application.Commands.DeleteJob;
using SchedulerJobCenter.Application.Commands.PauseJob;
using SchedulerJobCenter.Application.Commands.ResumeJob;
using SchedulerJobCenter.Application.Commands.TriggerJob;
using SchedulerJobCenter.Application.Commands.UpdateJob;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Application.Queries.GetJobById;
using SchedulerJobCenter.Application.Queries.GetJobs;
using SchedulerJobCenter.Application.Queries.ValidateCron;
using SchedulerJobCenter.Shared;

namespace SchedulerJobCenter.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class JobsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 20;

            var result = await mediator.Send(new GetJobsQuery(page, pageSize, keyword), ct);
            return Ok(ApiResponse<PagedResult<JobDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetJob(Guid id, CancellationToken ct = default)
        {
            var job = await mediator.Send(new GetJobByIdQuery(id), ct);
            return job is null
                ? NotFound(ApiResponse.Fail($"Job {id} not found."))
                : Ok(ApiResponse<JobDto>.Ok(job));
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob(
            [FromBody] CreateJobCommand cmd,
            CancellationToken ct = default)
        {
            var job = await mediator.Send(cmd, ct);
            return CreatedAtAction(nameof(GetJob), new { id = job.Id },
                ApiResponse<JobDto>.Ok(job, "Job created successfully."));
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateJob(
            Guid id,
            [FromBody] UpdateJobCommand cmd,
            CancellationToken ct = default)
        {
            var job = await mediator.Send(cmd with { Id = id }, ct);
            return Ok(ApiResponse<JobDto>.Ok(job, "Job updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteJob(Guid id, CancellationToken ct = default)
        {
            var result = await mediator.Send(new DeleteJobCommand(id), ct);
            return result
                ? Ok(ApiResponse.Ok("Job deleted successfully."))
                : NotFound(ApiResponse.Fail($"Job {id} not found."));
        }

        [HttpPost("{id:guid}/pause")]
        public async Task<IActionResult> PauseJob(Guid id, CancellationToken ct = default)
        {
            var result = await mediator.Send(new PauseJobCommand(id), ct);
            return result
                ? Ok(ApiResponse.Ok("Job paused successfully."))
                : NotFound(ApiResponse.Fail($"Job {id} not found."));
        }

        [HttpPost("{id:guid}/resume")]
        public async Task<IActionResult> ResumeJob(Guid id, CancellationToken ct = default)
        {
            var result = await mediator.Send(new ResumeJobCommand(id), ct);
            return result
                ? Ok(ApiResponse.Ok("Job resumed successfully."))
                : NotFound(ApiResponse.Fail($"Job {id} not found."));
        }

        [HttpPost("{id:guid}/trigger")]
        public async Task<IActionResult> TriggerJob(Guid id, CancellationToken ct = default)
        {
            var result = await mediator.Send(new TriggerJobCommand(id), ct);
            return result
                ? Ok(ApiResponse.Ok("Job triggered successfully."))
                : NotFound(ApiResponse.Fail($"Job {id} not found."));
        }


        [HttpGet("validate-cron")]
        public async Task<IActionResult> ValidateCron(
          [FromQuery] string cron,
          [FromQuery] string timeZoneId = "UTC",
          CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(cron))
                return BadRequest(ApiResponse.Fail("Cron expression is required."));

            var result = await mediator.Send(new ValidateCronQuery(cron, timeZoneId), ct);
            return Ok(ApiResponse<CronValidationDto>.Ok(result));
        }

        [HttpGet("timezones")]
        public IActionResult GetTimeZones()
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new
                {
                    tz.Id,
                    tz.DisplayName,
                    OffsetSign = tz.BaseUtcOffset < TimeSpan.Zero ? "-" : "+",
                    BaseUtcOffset = tz.BaseUtcOffset.ToString(@"hh\:mm")
                })
                .OrderBy(tz => tz.DisplayName);

            return Ok(ApiResponse<object>.Ok(timeZones));
        }
    }
}
