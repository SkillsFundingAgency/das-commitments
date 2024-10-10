using FluentValidation;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Validators;

public class GetCohortsRequestValidator : AbstractValidator<GetCohortsRequest>
{
    public GetCohortsRequestValidator()
    {
        RuleFor(request => request).Must(r => r.ProviderId != null || r.AccountId != null).WithMessage("The Account Id or Provider Id must be supplied");
    }
}