using FluentValidation;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;

public class GetApprenticeshipsQueryValidator : AbstractValidator<GetApprenticeshipsQuery>
{
    public GetApprenticeshipsQueryValidator()
    {
        RuleFor(request => request.SortField)
            .Must(field => string.IsNullOrEmpty(field) || 
                           typeof(Apprenticeship).GetProperties().Select(c => c.Name).Contains(field) ||
                           typeof(Cohort).GetProperties().Select(c => c.Name).Contains(field) ||
                           typeof(AccountLegalEntity).GetProperties().Select(c => c.Name).Contains(field) ||
                           typeof(ApprenticeshipConfirmationStatus).GetProperties().Select(c => c.Name).Contains(field) ||
                           field.Equals("ProviderName", StringComparison.CurrentCultureIgnoreCase))
            .WithMessage("Sort field must be empty or property on Apprenticeship ");
    }
}