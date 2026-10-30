using FluentValidation;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class CocApprovalValidator : AbstractValidator<CocApprovalDetails>
{
    public CocApprovalValidator()
    {
        RuleFor(x => x.Apprenticeship)
            .NotNull()
            .WithMessage("No Matching Apprenticeship Found");

        RuleFor(x => x.ULN)
            .Equal(x => x.Apprenticeship.Uln)
            .When(x => x.Apprenticeship != null)
            .WithMessage("The ULN does not match with the ApprenticeshipId assigned");

        RuleFor(x => x.LearningType)
            .Must((parent, change) => EnsureLearningTypeMatchesCourseLearningType(parent.Course.LearningType, change))
            .When(x => x.Apprenticeship != null && x.Course != null)
            .WithMessage("The LearningType does not match with the ApprenticeshipId assigned");

        RuleFor(x => x.ProviderId)
            .Equal(x => x.Apprenticeship.Cohort.ProviderId)
            .When(x => x.Apprenticeship != null)
            .WithMessage("The UKPRN does not match Provider assigned");

        bool EnsureLearningTypeMatchesCourseLearningType(LearningType? courseLearningType, CocLearningType cocLearningType)
        {
            courseLearningType ??= LearningType.Apprenticeship;

            return cocLearningType.ToString().Equals(courseLearningType.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
