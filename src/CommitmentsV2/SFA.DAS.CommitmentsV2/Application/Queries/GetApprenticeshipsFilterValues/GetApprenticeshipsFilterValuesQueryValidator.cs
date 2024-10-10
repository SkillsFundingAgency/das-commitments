using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;

public class GetApprenticeshipsFilterValuesQueryValidator :  AbstractValidator<GetApprenticeshipsFilterValuesQuery>
{
    public GetApprenticeshipsFilterValuesQueryValidator()
    {
        Unless(request => request.EmployerAccountId.HasValue && request.EmployerAccountId.Value > 0, () =>
        {
            RuleFor(request => request.ProviderId)
                .Must(id => id.HasValue && id.Value > 0)
                .WithMessage("The provider id must be set");
        });

        Unless(request => request.ProviderId.HasValue && request.ProviderId.Value > 0, () =>
        {
            RuleFor(request => request.EmployerAccountId)
                .Must(id => id.HasValue && id.Value > 0)
                .WithMessage("The employer account id must be set");
        });

        When(request => request.ProviderId.HasValue && request.EmployerAccountId.HasValue, () =>
        {
            Unless(request => request.EmployerAccountId.Value == 0, () =>
            {
                RuleFor(request => request.ProviderId)
                    .Must(id => id.Value == 0)
                    .WithMessage("The provider id must be zero if employer account id is set");
            });

            Unless(request => request.ProviderId.Value == 0, () =>
            {
                RuleFor(request => request.EmployerAccountId)
                    .Must(id => id.Value == 0)
                    .WithMessage("The employer account id must be zero if provider id is set");
            });
        });
    }
}