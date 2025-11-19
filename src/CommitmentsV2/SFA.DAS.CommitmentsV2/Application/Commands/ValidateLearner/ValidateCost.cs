using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private static IEnumerable<LearnerError> ValidateCost(LearnerDataEnhanced record)
    {
        var domainErrors = new List<LearnerError>();
        if (record.Cost <= 0 || record.Cost > Constants.MaximumApprenticeshipCost)
        {
            domainErrors.Add(new LearnerError("Cost", "Total agreed apprenticeship price cannot be £0 - re-submit your ILR file with correct training price (TNP1) and end-point assessment price (TNP2)"));
        }
        
        if ((record.TrainingPrice <= 0 || record.TrainingPrice > Constants.MaximumApprenticeshipCost) && record.Cost > 0)
        {
            domainErrors.Add(new LearnerError("TrainingPrice", "Training price (TNP1) must be in the range of 1-100000 - re-submit your ILR file with correct training price"));
        }

        if ((record.EpaoPrice < 0 || record.EpaoPrice > Constants.MaximumApprenticeshipCost) && record.Cost > 0)
        {
            domainErrors.Add(new LearnerError("EpaoPrice", "Endpoint assessment price (TNP2) must be in the range of 1-100000 - re-submit your ILR file with the correct price"));
        }

        return domainErrors;
    }
}