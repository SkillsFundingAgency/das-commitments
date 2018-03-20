using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateCommitmentAgreementCommandValidator : AbstractValidator<UpdateCommitmentAgreementCommand>
    {
        public UpdateCommitmentAgreementCommandValidator()
        {
            RuleFor(x => x.LatestAction).IsInEnum();
            RuleFor(x => x.Caller).NotNull();

            When(x => x.Caller != null, () => 
            {
                RuleFor(x => x.Caller.Id).GreaterThan(0);
                RuleFor(x => x.Caller.CallerType).IsInEnum();
            });

            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.LastUpdatedByName).NotEmpty();
            RuleFor(x => x.LastUpdatedByEmail).NotNull().Matches(Resources.EmailRegEx);
        }
    }
}
