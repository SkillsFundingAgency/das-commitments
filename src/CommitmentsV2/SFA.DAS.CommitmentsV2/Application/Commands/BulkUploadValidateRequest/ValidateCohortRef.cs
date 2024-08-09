using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.ProviderRelationships.Api.Client;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private const string CohortRefPermissionIssue = "CohortRefPermission";

    private async Task<List<Error>> ValidateCohortRef(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId)
    {
        var domainErrors = new List<Error>();

        if (string.IsNullOrWhiteSpace(csvRecord.CohortRef))
        {
            return domainErrors;
        }

        var cohort = GetCohortDetails(csvRecord.CohortRef);

        if (cohort == null)
        {
            domainErrors.Add(new Error("CohortRef", $"Enter a valid <b>Cohort Ref</b>"));
            return domainErrors;
        }

        if (csvRecord.CohortRef.Length > 20)
        {
            domainErrors.Add(new Error("CohortRef", $"Enter a valid <b>Cohort Ref</b>"));
        }
        else if (cohort.AccountLegalEntity.PublicHashedId != csvRecord.AgreementId && !string.IsNullOrWhiteSpace(await GetEmployerName(csvRecord.AgreementId)))
        {
            domainErrors.Add(new Error("CohortRef", $"Enter a valid <b>Cohort Ref</b>"));
        }
        else if (cohort.ProviderId != providerId)
        {
            domainErrors.Add(new Error("CohortRef", $"Enter a valid <b>Cohort Ref</b>"));
        }

        if (cohort.WithParty == Types.Party.Employer)
        {
            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort, as it is with the employer. You need to <b>add this learner to a different or new cohort.</b>"));
        }
        if (cohort.WithParty == Types.Party.TransferSender)
        {
            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort, as it is with the transfer sending employer. You need to <b>add this learner to a different or new cohort.</b>"));
        }
        if (cohort.IsLinkedToChangeOfPartyRequest)
        {
            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort. You need to <b>add this learner to a different or new cohort.</b>"));
        }
        if (cohort.DraftApprenticeships.Any(x => x.Uln == csvRecord.Uln))
        {
            domainErrors.Add(new Error("CohortRef", $"The <b>unique learner number</b> has already been used for an apprentice in this cohort."));
        }
        if (cohort.DraftApprenticeships.Any(x => x.Email == csvRecord.Email))
        {
            domainErrors.Add(new Error("CohortRef", $"The <b>email address</b> has already been used for an apprentice in this cohort."));
        }
        if (cohort.DraftApprenticeships.Any() &&
            cohort.DraftApprenticeships.Any(draftApprenticeship =>
                draftApprenticeship.FirstName == null || draftApprenticeship.LastName == null || draftApprenticeship.DateOfBirth == null ||
                draftApprenticeship.StartDate == null || draftApprenticeship.EndDate == null ||
                draftApprenticeship.CourseName == null ||
                draftApprenticeship.Cost == null ||
                draftApprenticeship.Uln == null ||
                draftApprenticeship.Email == null))
        {
            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to {csvRecord.CohortRef}, as this cohort contains incomplete records. You need to <b>complete all details</b> before you can add into this cohort."));
        }

        var overlapUlnResult = OverlapUlnCheckForCohort(cohort);
        if (overlapUlnResult.Result != null && overlapUlnResult.Result.Exists(x => x.HasOverlaps))
        {
            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to {csvRecord.CohortRef}, as this cohort contains an overlapping training date. You need to <b>resolve any overlapping training date errors</b> before you can add into this cohort."));
        }

        var overlapEmailResult = OverlapEmailCheckForCohort(cohort);
        if (overlapEmailResult.Result != null && overlapEmailResult.Result.Exists(x => x.OverlapStatus != OverlapStatus.None))
        {
            domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to {csvRecord.CohortRef} as it contains an overlapping email address. You need to <b>enter a unique email address</b> before you can add into this cohort."));
        }

        return domainErrors;
    }

    private async Task<bool> ValidatePermissionToCreateCohort(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId, ICollection<Error> domainErrors, bool? isLevy)
    {
        const string nonLevyPermissionText = "You do not have permission to <b>add apprentice records</b> for this employer, so you cannot <b>reserve funds</b> on their behalf";
        const string levyPermissionText = "The <b>employer must give you permission</b> to add apprentices on their behalf";

        var hasPermissionToCreateCohort = await HasPermissionToCreateCohort(csvRecord, providerId);
        if (!hasPermissionToCreateCohort)
        {
            var errorTextToUse = (isLevy.HasValue && isLevy.Value) ? levyPermissionText : nonLevyPermissionText;
            _logger.LogInformation("Has permission to create cohort : {ProviderId}", providerId);
            domainErrors.Add(new Error(CohortRefPermissionIssue, errorTextToUse));
        }

        return hasPermissionToCreateCohort;
    }

    private async Task<bool> HasPermissionToCreateCohort(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId)
    {
        var employerDetails = await GetEmployerDetails(csvRecord.AgreementId);

        if (!employerDetails.LegalEntityId.HasValue || providerId == 0)
        {
            return true;
        }

        _logger.LogInformation("Checking permission for Legal entity :{employerDetails.LegalEntityId.Value} -- ProviderId : {ProviderId}", employerDetails.LegalEntityId.Value, providerId);
        var request = new HasPermissionRequest
        {
            AccountLegalEntityId = employerDetails.LegalEntityId.Value,
            Operation = Operation.CreateCohort,
            Ukprn = providerId
        };

        var result = await _providerRelationshipsApiClient.HasPermission(request);
        _logger.LogInformation("Checking permission for Legal entity :{employerDetails.LegalEntityId.Value} -- ProviderId : {providerId} -- result {result}", employerDetails.LegalEntityId.Value, providerId, result);

        employerDetails.HasPermissionToCreateCohort = result;
        return result;
    }

    private async Task<List<OverlapCheckResult>> OverlapUlnCheckForCohort(Models.Cohort cohort)
    {
        return await _overlapService.CheckForOverlaps(cohort.Id, CancellationToken.None);
    }

    private async Task<List<EmailOverlapCheckResult>> OverlapEmailCheckForCohort(Models.Cohort cohort)
    {
        return await _overlapService.CheckForEmailOverlaps(cohort.Id, CancellationToken.None);
    }
}