using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdateStatus;

public class GetApprenticeshipUpdateStatusQueryValidator : AbstractValidator<GetApprenticeshipUpdateStatusQuery>
{
    public GetApprenticeshipUpdateStatusQueryValidator()
    {
        RuleFor(q => q.ApprenticeshipId).GreaterThan(0);
    }
}