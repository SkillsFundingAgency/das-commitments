using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship
{
    public class StopApprenticeshipCommandHandler : AsyncRequestHandler<StopApprenticeshipCommand>
    {
        public StopApprenticeshipCommandHandler(IApprenticeshipDomainService)
        {
        }

        protected override Task Handle(StopApprenticeshipCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
            return _apprenticeshipDomainService
            //return _cohortDomainService.SendCohortToOtherParty(request.CohortId, request.Message, request.UserInfo, cancellationToken);
        }
    }
}
