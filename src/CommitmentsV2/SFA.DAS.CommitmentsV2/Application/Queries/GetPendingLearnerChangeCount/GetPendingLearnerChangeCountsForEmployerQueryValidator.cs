using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingLearnerChangeCount;

public class GetPendingLearnerChangeCountsForEmployerQueryValidator : AbstractValidator<GetPendingLearnerChangeCountsForEmployerQuery>
{
    public GetPendingLearnerChangeCountsForEmployerQueryValidator()
    {
        RuleFor(x => x.EmployerAccountId).GreaterThan(0);
    }
}