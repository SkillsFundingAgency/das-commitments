using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Reference;

public class DraftApprenticeshipSetReferenceCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DraftApprenticeshipSetReferenceCommandHandler> logger)
    : IRequestHandler<DraftApprenticeshipSetReferenceCommand>
{
    public async Task Handle(DraftApprenticeshipSetReferenceCommand command, CancellationToken cancellationToken)
    {

        try
        {
            var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

            apprenticeship.SetReference(command.Reference, command.Party);


            logger.LogInformation("Set reference for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);
        }

        catch (Exception e)
        {
            logger.LogError(e, "Error Adding Transfer Request");
            throw;
        }
    }
}