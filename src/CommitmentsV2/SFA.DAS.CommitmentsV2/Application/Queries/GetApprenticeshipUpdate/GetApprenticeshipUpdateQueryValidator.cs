using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;

public class GetApprenticeshipUpdateQueryValidator : AbstractValidator<GetApprenticeshipUpdateQuery>
{
    public GetApprenticeshipUpdateQueryValidator()
    {
        RuleFor(q => q.ApprenticeshipId).GreaterThan(0);
    }
}