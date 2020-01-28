using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort
{
    public class ApproveCohortCommandHandler : AsyncRequestHandler<ApproveCohortCommand>
    {
        private readonly ICohortDomainService _cohortDomainService;

        public ApproveCohortCommandHandler(ICohortDomainService cohortDomainService)
        {
            _cohortDomainService = cohortDomainService;
        }

        protected override Task Handle(ApproveCohortCommand request, CancellationToken cancellationToken)
        {
            return _cohortDomainService.ApproveCohort(request.CohortId, request.Message, request.UserInfo, cancellationToken);
        }
    }
}