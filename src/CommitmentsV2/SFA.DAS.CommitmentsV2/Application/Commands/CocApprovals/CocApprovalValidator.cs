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

        RuleForEach(x => x.ApprovalFieldChanges)
                    .Must((parent, change) => EnsureEffectiveFromDateIsAfterCourseStartDate(change.Data.EffectiveFromDate, parent.Apprenticeship.StartDate))
                    .When(x => x.Apprenticeship != null)
                    .WithMessage("The effective from date cannot be prior to the start of the course");

        RuleForEach(x => x.ApprovalFieldChanges)
                    .Must((parent, change) => EnsureEffectiveFromDateIsBeforeEndDatesOnApprenticeship(change.Data.EffectiveFromDate, parent.Apprenticeship))
                    .When(x => x.Apprenticeship != null)
                    .WithMessage("The effective from date cannot be after the end of the course");

        bool EnsureEffectiveFromDateIsAfterCourseStartDate(DateTime? effectiveFromDate, DateTime? StartDate)
        {
            if (!effectiveFromDate.HasValue || !StartDate.HasValue)
            {
                return true;
            }
            return effectiveFromDate.Value >= StartDate.Value;
        }

        bool EnsureEffectiveFromDateIsBeforeEndDatesOnApprenticeship(DateTime? effectiveFromDate, Apprenticeship apprenticeship)
        {
            if (!effectiveFromDate.HasValue)
            {
                return true;
            }

            var bestEndDate = apprenticeship.CompletionDate ?? apprenticeship.StopDate ?? apprenticeship.EndDate;

            return effectiveFromDate.Value <= bestEndDate;
        }

        bool EnsureLearningTypeMatchesCourseLearningType(LearningType? courseLearningType, CocLearningType cocLearningType)
        {
            courseLearningType ??= LearningType.Apprenticeship;

            return cocLearningType.ToString().Equals(courseLearningType.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
