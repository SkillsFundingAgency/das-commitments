using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private static IEnumerable<LearnerError> ValidateGivenName(LearnerDataEnhanced record)
    {
        var domainErrors = new List<LearnerError>();
        if (string.IsNullOrEmpty(record.FirstName))
        {
            domainErrors.Add(new LearnerError("GivenName", "First name must not be blank"));
        }
        else if (record.FirstName.Length > 100)
        {
            domainErrors.Add(new LearnerError("GivenName", "First name cannot be longer than 100 characters"));
        }

        return domainErrors;
    }
}