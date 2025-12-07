using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Reference;

public class DraftApprenticeshipSetReferenceCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DraftApprenticeshipSetReferenceCommandHandler> logger,
    IViewEditDraftApprenticeshipReferenceValidationService service)
    : IRequestHandler<DraftApprenticeshipSetReferenceCommand, DraftApprenticeshipSetReferenceResult>
{
    public async Task<DraftApprenticeshipSetReferenceResult> Handle(DraftApprenticeshipSetReferenceCommand command, CancellationToken cancellationToken)
    {

        try
        {
            var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

           await service.Validate(new Domain.Entities.ViewEditDraftApprenticeshipReferenceValidationRequest()
            {
                CohortId = command.CohortId,
                DraftApprenticeshipId = command.ApprenticeshipId,
                Party = command.Party,
                Reference = command.Reference,
            }, cancellationToken);

            apprenticeship.SetReference(command.Reference, command.Party);


            logger.LogInformation("Set reference for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);

            return new DraftApprenticeshipSetReferenceResult() { DraftApprenticeshipId = command.ApprenticeshipId };
        }

        catch (Exception e)
        {
            logger.LogError(e, "Error Adding Transfer Request");
            throw;
        }
    }
}