using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateCommitmentAgreementCommandValidator : AbstractValidator<UpdateCommitmentAgreementCommand>
    {
        // This is the same Regex as used in the Employer site.
        private static readonly Regex emailRegEx = new Regex(
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

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
            RuleFor(x => x.LastUpdatedByEmail).NotNull().Matches(emailRegEx);
        }
    }
}
