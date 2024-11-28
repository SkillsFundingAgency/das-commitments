using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;

public class GetCohortsQueryValidator :  AbstractValidator<GetCohortsQuery>
{
    public GetCohortsQueryValidator()
    {
        RuleFor(model => model).Must(m=>m.ProviderId != null || m.AccountId != null).WithMessage("The Account Id or Provider Id must be supplied");
    }
}