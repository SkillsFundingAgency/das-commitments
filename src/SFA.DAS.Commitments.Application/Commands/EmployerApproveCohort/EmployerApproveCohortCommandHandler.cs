using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.EmployerApproveCohort
{
    public sealed class EmployerApproveCohortCommandHandler : AsyncRequestHandler<EmployerApproveCohortCommand>
    {
        protected override Task HandleCore(EmployerApproveCohortCommand message)
        {
            throw new System.NotImplementedException();
        }
    }
}
