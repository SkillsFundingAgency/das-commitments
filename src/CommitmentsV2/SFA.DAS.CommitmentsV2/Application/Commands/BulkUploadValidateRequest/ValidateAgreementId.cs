using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    public const string LegalAgreementIdIssue = "LegalAgreementId";
    private async Task<List<Error>> ValidateAgreementIdValidFormat(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        List<Error> errors = new List<Error>();
        if (string.IsNullOrEmpty(csvRecord.AgreementId))
        {
            errors.Add(new Error("AgreementId", "<b>Agreement ID</b> must be entered"));
        }
        else if (!csvRecord.AgreementId.All(char.IsLetterOrDigit))
        {
            errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        }
        else if (csvRecord.AgreementId.Length > 6)
        {
            errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        }
        else if (string.IsNullOrWhiteSpace(await GetEmployerName(csvRecord.AgreementId)))
        {
            errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        }

        return errors;
    }

    private async Task<List<Error>> ValidateAgreementIdIsSigned(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        List<Error> errors = new List<Error>();
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