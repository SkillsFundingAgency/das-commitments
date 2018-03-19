using FluentValidation;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort
{
    public sealed class ProviderApproveCohortCommandValidator : AbstractValidator<ProviderApproveCohortCommand>
    {
        public ProviderApproveCohortCommandValidator()
        {
            RuleFor(x => x.Caller).NotNull();

            When(x => x.Caller != null, () => 
            {
                RuleFor(x => x.Caller.Id).GreaterThan(0);
                RuleFor(x => x.Caller.CallerType).Equal(CallerType.Provider);
            });

            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.LastUpdatedByName).NotEmpty();
            RuleFor(x => x.LastUpdatedByEmail).NotNull().Matches(Resources.EmailRegEx);
        }
    }
}
