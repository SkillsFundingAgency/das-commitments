using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Reference;

public class DraftApprenticeshipSetReferenceCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DraftApprenticeshipSetReferenceCommandHandler> logger,
    IViewEditDraftApprenticeshipReferenceValidationService service)
    : IRequestHandler<DraftApprenticeshipSetReferenceCommand, DraftApprenticeshipSetReferenceResult>
{
    public async Task<DraftApprenticeshipSetReferenceResult> Handle(DraftApprenticeshipSetReferenceCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        var validationResult = await service.Validate(new Domain.Entities.ViewEditDraftApprenticeshipReferenceValidationRequest()
        {
            CohortId = command.CohortId,
            DraftApprenticeshipId = command.ApprenticeshipId,
            Party = command.Party,
            Reference = command.Reference,
        }, cancellationToken);

        validationResult.Errors.ThrowIfAny();

        apprenticeship.SetReference(command.Reference, command.Party);

        logger.LogInformation("Set reference for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);

        return new DraftApprenticeshipSetReferenceResult() { DraftApprenticeshipId = command.ApprenticeshipId };
    }
}