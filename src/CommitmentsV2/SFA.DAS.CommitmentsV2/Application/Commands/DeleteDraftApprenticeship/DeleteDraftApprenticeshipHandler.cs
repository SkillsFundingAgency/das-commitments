using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;

public class DeleteDraftApprenticeshipHandler(
    ILogger<DeleteDraftApprenticeshipHandler> logger,
    ICohortDomainService cohortDomainService)
    : IRequestHandler<DeleteDraftApprenticeshipCommand>
{
    public async Task Handle(DeleteDraftApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await cohortDomainService.DeleteDraftApprenticeship(command.CohortId, command.ApprenticeshipId, command.UserInfo, cancellationToken);

            logger.LogInformation("Deleted apprenticeShip. Apprenticeship-Id:{ApprenticeshipId} Cohort-Id:{CohortId}", command.ApprenticeshipId, command.CohortId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error Deleting Apprenticeship");
            throw;
        }
    }
}