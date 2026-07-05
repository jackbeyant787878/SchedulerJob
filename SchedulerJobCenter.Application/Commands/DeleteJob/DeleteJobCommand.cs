using MediatR;
namespace SchedulerJobCenter.Application.Commands.DeleteJob
{
    public record DeleteJobCommand(Guid Id) : IRequest<bool>;
}
