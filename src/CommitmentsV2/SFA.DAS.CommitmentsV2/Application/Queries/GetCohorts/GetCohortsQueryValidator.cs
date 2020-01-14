using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts
{
    public class GetCohortsQueryValidator :  AbstractValidator<GetCohortsQuery>
    {
        public GetCohortsQueryValidator()
        {
            RuleFor(model => model.AccountId).NotNull().WithMessage("The Account Id must be supplied");
        }
    }
}
