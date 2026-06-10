using FluentValidation;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;

public class CocApprovalValidator : AbstractValidator<CocApprovalDetails>
{
    public CocApprovalValidator()
    {
        RuleFor(x => x.Apprenticeship)
            .NotNull()
            .WithMessage("No Matching Apprenticeship Found");

        RuleFor(x => x.Apprenticeship.Cohort.ProviderId)
            .Equal(x => x.ProviderId)
            .When(x => x.Apprenticeship != null)
            .WithMessage("The UKPRN does not match Provider assigned");

        RuleForEach(x => x.ApprovalFieldChanges)
                    .Must((parent, change) => EnsureEffectiveFromDateIsAfterCourseStartDate(change.Data.EffectiveFromDate, parent.Apprenticeship.StartDate))
                    .When(x => x.Apprenticeship != null)
                    .WithMessage("The effective from date cannot be prior to the start of the course");

        bool EnsureEffectiveFromDateIsAfterCourseStartDate(DateTime? effectiveFromDate, DateTime? StartDate)
        {
            if (!effectiveFromDate.HasValue || !StartDate.HasValue)
            {
                return true;
            }
            return effectiveFromDate.Value >= StartDate.Value;
        }
    }
}
