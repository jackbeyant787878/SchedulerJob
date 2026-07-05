using MediatR;
using SchedulerJobCenter.Application.DTOs;
namespace SchedulerJobCenter.Application.Queries.GetJobById
{
    public record GetJobByIdQuery(Guid Id) : IRequest<JobDto?>;
}
