using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private static IEnumerable<LearnerError> ValidateFamilyName(LearnerDataEnhanced record)
    {
        var domainErrors = new List<LearnerError>();
        if (string.IsNullOrEmpty(record.LastName))
        {
            domainErrors.Add(new LearnerError("LastName", "Last name must be entered"));
        }
        else if (record.LastName.Length > 100)
        {
            domainErrors.Add(new LearnerError("LastName", "Last name cannot be longer than 100 characters"));
        }

        return domainErrors;
    }
}