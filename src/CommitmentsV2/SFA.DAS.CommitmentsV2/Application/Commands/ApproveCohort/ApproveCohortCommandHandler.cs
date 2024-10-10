using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort;

public class ApproveCohortCommandHandler(ICohortDomainService cohortDomainService) : IRequestHandler<ApproveCohortCommand>
{
    public Task Handle(ApproveCohortCommand request, CancellationToken cancellationToken)
    {
        return cohortDomainService.ApproveCohort(request.CohortId, request.Message, request.UserInfo, request.RequestingParty, cancellationToken);
    }
}