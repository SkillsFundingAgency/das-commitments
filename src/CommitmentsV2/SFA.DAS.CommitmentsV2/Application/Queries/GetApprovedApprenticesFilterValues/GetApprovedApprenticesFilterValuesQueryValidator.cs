using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues
{
    public class GetApprovedApprenticesFilterValuesQueryValidator :  AbstractValidator<GetApprovedApprenticesFilterValuesQuery>
    {
        public GetApprovedApprenticesFilterValuesQueryValidator()
        {
           RuleFor(model => model.ProviderId).Must(id => id > 0).WithMessage("The Provider ID must be positive");
        }
    }
}
