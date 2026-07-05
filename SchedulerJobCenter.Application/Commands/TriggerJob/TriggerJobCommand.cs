using MediatR;
namespace SchedulerJobCenter.Application.Commands.TriggerJob
{
    public record TriggerJobCommand(Guid Id) : IRequest<bool>;
}
