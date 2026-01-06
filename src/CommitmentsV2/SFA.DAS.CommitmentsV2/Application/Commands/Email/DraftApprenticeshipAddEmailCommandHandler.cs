using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.EmailValidationService;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Email;

public class DraftApprenticeshipAddEmailCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DraftApprenticeshipAddEmailCommandHandler> logger,
    IOverlapCheckService overlapCheckService)
    : IRequestHandler<DraftApprenticeshipAddEmailCommand>
{
    public async Task Handle(DraftApprenticeshipAddEmailCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        var response = await Validate(command, apprenticeship, cancellationToken);

        response?.Errors.ThrowIfAny();

        apprenticeship?.SetEmail(command.Email);

        logger.LogInformation("Set Email  for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);
    }

    public async Task<ViewEditDraftApprenticeshipEmailValidationResult> Validate(DraftApprenticeshipAddEmailCommand command, DraftApprenticeship apprenticeship, CancellationToken cancellationToken)
    {
        var errors = new List<DomainError>();

        var cohort = dbContext.Value.Cohorts.FirstOrDefault(x => x.Id == command.CohortId);

        if (cohort == null)
        {
            throw new ApplicationException($"CohortId {command.CohortId} not found");
        }

        if (apprenticeship == null)
        {
            throw new ApplicationException($"ApprenticeshipId {apprenticeship.Id} not found");
        }

        if (errors.Count == 0)
        {
            errors.AddRange(BuildEmailValidationFailures(command, apprenticeship, cohort));
        }

        if (errors.Count == 0)
        {
            var overlapError = await EmailOverlapValidationFailures(command, apprenticeship);

            if (overlapError != null)
            {
                errors.Add(overlapError);
            }
        }

        return new ViewEditDraftApprenticeshipEmailValidationResult()
        {
            Errors = errors
        };
    }
    private async Task<DomainError> EmailOverlapValidationFailures(DraftApprenticeshipAddEmailCommand command, DraftApprenticeship apprenticeshipDetails)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return null;

        if (apprenticeshipDetails.StartDate == null || apprenticeshipDetails.EndDate == null)
            return null;

        var startDate = apprenticeshipDetails.StartDate.Value.Date;
        var endDate = apprenticeshipDetails.EndDate.Value.Date;

        var range = startDate.To(endDate);

        var overlap = await overlapCheckService.CheckForEmailOverlaps(command.Email, range, command.ApprenticeshipId, command.CohortId, CancellationToken.None);

        if (overlap != null)
        {
            return new DomainError(nameof(command.Email), overlap.BuildErrorMessage());
        }

        return null;
    }

    private IEnumerable<DomainError> BuildEmailValidationFailures(DraftApprenticeshipAddEmailCommand command, DraftApprenticeship apprenticeshipDetails, Cohort cohort)
    {

        if (cohort.WithParty != command.Party)
        {
            yield return new DomainError(nameof(command.Email), "You cannot update the Email value as the cohort is not assigned to you");
        }

        if (apprenticeshipDetails.Email == null && !string.IsNullOrWhiteSpace(command.Email) && apprenticeshipDetails.Cohort.EmployerAndProviderApprovedOn < new DateTime(2021, 09, 10))
        {
            yield return new DomainError(nameof(command.Email), "Email update cannot be requested");
        }

        if (command.Email != apprenticeshipDetails.Email && !string.IsNullOrWhiteSpace(command.Email))
        {
            if (!command.Email.IsAValidEmailAddress())
            {
                yield return new DomainError(nameof(command.Email), "Please enter a valid email address");
            }
        }
    }
}