using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private static IEnumerable<LearnerError> ValidateEndDate(LearnerDataEnhanced record)
    {
        var domainErrors = new List<LearnerError>();

        if (record.PlannedEndDate < record.StartDate)
        {
            domainErrors.Add(new LearnerError("PlannedEndDate", "Enter a planned end date that is after the start date"));
        }

        return domainErrors;
    }
}