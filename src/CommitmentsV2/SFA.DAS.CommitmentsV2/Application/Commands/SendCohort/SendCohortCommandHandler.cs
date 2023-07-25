using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.SendCohort
{
    public class SendCohortCommandHandler : IRequestHandler<SendCohortCommand>
    {
        private readonly ICohortDomainService _cohortDomainService;

        public SendCohortCommandHandler(ICohortDomainService cohortDomainService)
        {
            _cohortDomainService = cohortDomainService;
        }

        public Task Handle(SendCohortCommand request, CancellationToken cancellationToken)
        {
            return _cohortDomainService.SendCohortToOtherParty(request.CohortId, request.Message, request.UserInfo, request.RequestingParty, cancellationToken);
        }
    }
}