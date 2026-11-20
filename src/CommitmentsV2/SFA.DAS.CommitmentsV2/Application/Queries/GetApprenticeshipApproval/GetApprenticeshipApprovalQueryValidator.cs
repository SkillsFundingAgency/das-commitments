using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

public class GetApprenticeshipApprovalQueryValidator : AbstractValidator<GetApprenticeshipApprovalQuery>
{
    public GetApprenticeshipApprovalQueryValidator()
    {
        RuleFor(q => q.ApprenticeshipId).GreaterThan(0);
    }
}
