using System.Linq;
using FluentValidation;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsValidator : AbstractValidator<GetApprenticeshipsRequest>
    {
        public GetApprenticeshipsValidator()
        {
            RuleFor(request => request.ProviderId).Must(id => id > 0).WithMessage("The provider id must be set");
           
            RuleFor(request => request.SortField)
                .Must(field => string.IsNullOrEmpty(field) || 
                               typeof(Apprenticeship).GetProperties().Select(c => c.Name).Contains(field) ||
                               typeof(Cohort).GetProperties().Select(c => c.Name).Contains(field))
                .WithMessage("Sort field must be empty or property on Apprenticeship ");

        }
    }
}
