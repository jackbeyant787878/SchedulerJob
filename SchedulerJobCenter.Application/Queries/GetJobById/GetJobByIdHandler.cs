using MediatR;
using SchedulerJobCenter.Application.DTOs;
using SchedulerJobCenter.Infrastructure.Repositories;
namespace SchedulerJobCenter.Application.Queries.GetJobById
{
    public class GetJobByIdHandler(IJobRepository jobRepo) : IRequestHandler<GetJobByIdQuery, JobDto?>
    {
        public async Task<JobDto?> Handle(GetJobByIdQuery query, CancellationToken ct)
        {
            var job = await jobRepo.GetByIdAsync(query.Id, ct);
            return job is null ? null : JobDto.From(job);
        }
    }
}
