using MediatR;
using SchedulerJobCenter.Application.DTOs;

namespace SchedulerJobCenter.Application.Queries.ValidateCron
{
    public record ValidateCronQuery(string CronExpression,string TimeZoneId = "UTC") : IRequest<CronValidationDto>;
}
