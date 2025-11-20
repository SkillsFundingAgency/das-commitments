using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private const string LegalAgreementIdIssue = "LegalAgreementId";

    private async Task<List<Error>> ValidateAgreementIdValidFormat(LearnerDataEnhanced record)
    {
        var errors = new List<Error>();
        if (string.IsNullOrEmpty(record.AgreementId))
        {
            errors.Add(new Error("AgreementId", "Agreement ID must be entered"));
        }
        else if (!record.AgreementId.All(char.IsLetterOrDigit))
        {
            errors.Add(new Error("AgreementId", $"Enter a valid Agreement ID"));
        }
        else if (record.AgreementId.Length > 6)
        {
            errors.Add(new Error("AgreementId", $"Enter a valid Agreement ID"));
        }
        else if (string.IsNullOrWhiteSpace(await GetEmployerName(record.AgreementId)))
        {
            errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        }

        return errors;
    }

    private async Task<List<Error>> ValidateAgreementIdIsSigned(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        var errors = new List<Error>();
        if (!(await IsSigned(csvRecord.AgreementId)).GetValueOrDefault(false))
        {
            errors.Add(new Error(LegalAgreementIdIssue, "You cannot add apprentices for this employer as they need to <b>accept the agreement</b> with the DfE."));
        }

        return errors;
    }

    private async Task<bool?> IsSigned(string agreementId)
    {
        var employerDetails = await GetEmployerDetails(agreementId);
        return employerDetails.IsSigned;
    }
}