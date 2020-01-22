using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class GetCohortsRequestValidator : AbstractValidator<GetCohortsRequest>
    {
        public GetCohortsRequestValidator()
        {
            RuleFor(r => r.AccountId).NotNull();
        }
    }
}