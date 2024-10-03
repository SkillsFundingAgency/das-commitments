using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;

public class GetOverlappingTrainingDateRequestQueryValidator : AbstractValidator<GetOverlappingTrainingDateRequestQuery>
{
    public GetOverlappingTrainingDateRequestQueryValidator()
    {
        RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
    }
}