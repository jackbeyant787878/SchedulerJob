using MediatR;
namespace SchedulerJobCenter.Application.Commands.PauseJob
{
    public record PauseJobCommand(Guid Id) : IRequest<bool>;
}
