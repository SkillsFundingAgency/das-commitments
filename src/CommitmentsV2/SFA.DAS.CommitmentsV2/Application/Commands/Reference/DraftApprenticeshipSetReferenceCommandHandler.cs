using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Reference;

public class DraftApprenticeshipSetReferenceCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DraftApprenticeshipSetReferenceCommandHandler> logger)
    : IRequestHandler<DraftApprenticeshipSetReferenceCommand, DraftApprenticeshipSetReferenceResult>
{
    public async Task<DraftApprenticeshipSetReferenceResult> Handle(DraftApprenticeshipSetReferenceCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        var validationResult = Validate(command, apprenticeship, cancellationToken);

        validationResult.Errors.ThrowIfAny();

        apprenticeship.SetReference(command.Reference, command.Party);

        logger.LogInformation("Set reference for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);

        return new DraftApprenticeshipSetReferenceResult() { DraftApprenticeshipId = command.ApprenticeshipId };
    }

    public ViewEditDraftApprenticeshipReferenceValidationResult Validate(DraftApprenticeshipSetReferenceCommand command, DraftApprenticeship draftApprenticeship, CancellationToken cancellationToken)
    {
        var errors = new List<DomainError>();

        var cohort = dbContext.Value.Cohorts.FirstOrDefault(x => x.Id == command.CohortId);
        var apprenticeship = draftApprenticeship;

        if (cohort == null)
        {
            throw new ApplicationException($"CohortId {command.CohortId} not found");
        }

        if (apprenticeship == null)
        {
            throw new ApplicationException($"ApprenticeshipId {draftApprenticeship.Id} not found");
        }

        errors.AddRange(BuildRefValidationFailures(command, apprenticeship, cohort));


        return new ViewEditDraftApprenticeshipReferenceValidationResult()
        {
            Errors = errors
        };
    }

    private IEnumerable<DomainError> BuildRefValidationFailures(DraftApprenticeshipSetReferenceCommand command, DraftApprenticeship apprenticeshipDetails, Cohort cohort)
    {
        if (cohort.WithParty != command.Party)
        {
            yield return new DomainError(nameof(command.Reference), "You cannot update the Reference value as the cohort is not assigned to you");
        }

        if (command.Reference != null && command.Reference.Length > 20)
        {
            yield return new DomainError(nameof(command.Reference), "The Reference must be 20 characters or fewer");
        }
    }
}