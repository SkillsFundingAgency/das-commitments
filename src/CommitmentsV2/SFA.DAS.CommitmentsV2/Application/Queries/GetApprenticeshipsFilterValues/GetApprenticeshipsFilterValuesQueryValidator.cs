using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQueryValidator :  AbstractValidator<GetApprenticeshipsFilterValuesQuery>
    {
        public GetApprenticeshipsFilterValuesQueryValidator()
        {
           RuleFor(model => model.ProviderId).Must(id => id > 0).WithMessage("The Provider ID must be positive");
        }
    }
}
