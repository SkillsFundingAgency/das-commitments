using System.Linq;
using FluentValidation;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsQueryValidator : AbstractValidator<GetApprenticeshipsQuery>
    {
        public GetApprenticeshipsQueryValidator()
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


            RuleFor(request => request.SortField)
                .Must(field => string.IsNullOrEmpty(field) || 
                               typeof(Apprenticeship).GetProperties().Select(c => c.Name).Contains(field) ||
                               typeof(Cohort).GetProperties().Select(c => c.Name).Contains(field) ||
                               typeof(AccountLegalEntity).GetProperties().Select(c => c.Name).Contains(field) ||
                               typeof(Provider).GetProperties().Select(c => c.Name).Contains(field))
                .WithMessage("Sort field must be empty or property on Apprenticeship ");
        }
    }
}
