using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService
{
    private const string LegalAgreementIdIssue = "LegalAgreementId";

    public async Task<List<Error>> ValidateAgreementIdValidFormat(string agreementId, string employerName)
    {
        var errors = new List<Error>();
        if (string.IsNullOrEmpty(agreementId))
        {
            errors.Add(new Error("AgreementId", "<b>Agreement ID</b> must be entered"));
        }
        else if (!agreementId.All(char.IsLetterOrDigit))
        {
            errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        }
        else if (agreementId.Length > 6)
        {
            errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        }
        else if (string.IsNullOrWhiteSpace(employerName))
        {
            errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
        }

        return errors;
    }

    public async Task<List<Error>> ValidateAgreementIdIsSigned(bool? isSigned)
    {
        var errors = new List<Error>();
        if (!isSigned.GetValueOrDefault(false))
        {
            errors.Add(new Error(LegalAgreementIdIssue, "You cannot add apprentices for this employer as they need to <b>accept the agreement</b> with the DfE."));
        }

        return errors;
    }
}