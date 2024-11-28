using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.SendCohort;

public class SendCohortCommandHandler(ICohortDomainService cohortDomainService) : IRequestHandler<SendCohortCommand>
{
    public Task Handle(SendCohortCommand request, CancellationToken cancellationToken)
    {
        return cohortDomainService.SendCohortToOtherParty(request.CohortId, request.Message, request.UserInfo, request.RequestingParty, cancellationToken);
    }
}