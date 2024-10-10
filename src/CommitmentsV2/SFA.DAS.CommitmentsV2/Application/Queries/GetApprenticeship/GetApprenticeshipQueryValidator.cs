using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;

public class GetApprenticeshipQueryValidator : AbstractValidator<GetApprenticeshipQuery>
{
    public GetApprenticeshipQueryValidator()
    {
        RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
    }
}