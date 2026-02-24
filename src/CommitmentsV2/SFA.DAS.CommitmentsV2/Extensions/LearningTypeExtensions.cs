using CommonLearningType = SFA.DAS.Common.Domain.Types.LearningType;
using LocalLearningType = SFA.DAS.CommitmentsV2.Types.LearningType;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class LearningTypeExtensions
{
    public static LocalLearningType? ToLocalLearningType(this CommonLearningType? commonLearningType)
    {
        return commonLearningType switch
        {
            CommonLearningType.Apprenticeship => LocalLearningType.Apprenticeship,
            CommonLearningType.FoundationApprenticeship => LocalLearningType.FoundationApprenticeship,
            CommonLearningType.ApprenticeshipUnit => LocalLearningType.ApprenticeshipUnit,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(commonLearningType), commonLearningType, "Unknown LearningType value")
        };
    }

    public static CommonLearningType? ToCommonLearningType(this LocalLearningType? localLearningType)
    {
        return localLearningType switch
        {
            LocalLearningType.Apprenticeship => CommonLearningType.Apprenticeship,
            LocalLearningType.FoundationApprenticeship => CommonLearningType.FoundationApprenticeship,
            LocalLearningType.ApprenticeshipUnit => CommonLearningType.ApprenticeshipUnit,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(localLearningType), localLearningType, "Unknown LearningType value")
        };
    }
}