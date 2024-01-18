using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval
{
    public class GetEmployerCohortsReadyForApprovalQueryValidator : AbstractValidator<GetEmployerCohortsReadyForApprovalQuery>
    {
        public GetEmployerCohortsReadyForApprovalQueryValidator()
        {
            RuleFor(x => x.EmployerAccountId).GreaterThan(0);
        }
    }
}