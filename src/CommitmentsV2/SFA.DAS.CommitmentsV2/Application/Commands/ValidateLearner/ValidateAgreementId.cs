using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private const string LegalAgreementIdIssue = "LegalAgreementId";

    private async Task<List<LearnerError>> ValidateAgreementIdValidFormat(LearnerDataEnhanced record)
    {
        var errors = new List<LearnerError>();
        if (string.IsNullOrEmpty(record.AgreementId))
        {
            errors.Add(new LearnerError("AgreementId", "Agreement ID must be entered"));
        }
        else if (!record.AgreementId.All(char.IsLetterOrDigit) || (record.AgreementId.Length > 6))
        {
            errors.Add(new LearnerError("AgreementId", $"Invalid Agreement ID"));
        }
        else if (string.IsNullOrWhiteSpace(await GetEmployerName(record.AgreementId)))
        {
            errors.Add(new LearnerError("AgreementId", $"Invalid Agreement ID"));
        }

        return errors;
    }

    private async Task<List<LearnerError>> ValidateAgreementIdIsSigned(LearnerDataEnhanced record)
    {
        var errors = new List<LearnerError>();
        if (!(await IsSigned(record.AgreementId)).GetValueOrDefault(false))
        {
            errors.Add(new LearnerError(LegalAgreementIdIssue, "You cannot add apprentices for this employer as they need to accept the agreement with the DfE."));
        }

        return errors;
    }

    private async Task<bool?> IsSigned(string agreementId)
    {
        var employerDetails = await GetEmployerDetails(agreementId);
        return employerDetails.IsSigned;
    }
}