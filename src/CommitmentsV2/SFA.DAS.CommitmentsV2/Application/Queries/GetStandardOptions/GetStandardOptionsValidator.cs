using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetStandardOptions
{
    public class GetStandardOptionsValidator : AbstractValidator<GetStandardOptionsQuery>
    {
        public GetStandardOptionsValidator()
        {
            RuleFor(q => q.StandardUId).NotEmpty();
        }
    }
}
