using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;

namespace SFA.DAS.Commitments.Application.Rules
{
    public interface IApprenticeshipOverlapRules
    {
        ValidationFailReason DetermineOverlap(ApprenticeshipOverlapValidationRequest request, ApprenticeshipResult apprenticeship);
    }
}
