using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesValidator : AbstractValidator<GetApprovedApprenticesRequest>
    {
        public GetApprovedApprenticesValidator()
        {
            RuleFor(request => request.ProviderId).Must(id => id > 0).WithMessage("The provider id must be set");
        }
    }
}
