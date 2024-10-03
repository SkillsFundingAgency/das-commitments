using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;

public class GetDataLocksQueryValidator : AbstractValidator<GetDataLocksQuery>
{
    public GetDataLocksQueryValidator()
    {
        RuleFor(q => q.ApprenticeshipId).GreaterThan(0);
    }
}