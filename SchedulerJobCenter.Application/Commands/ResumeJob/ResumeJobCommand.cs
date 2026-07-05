using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobCenter.Application.Commands.ResumeJob
{
    public record ResumeJobCommand(Guid Id) : IRequest<bool>;
}
