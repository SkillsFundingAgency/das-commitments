using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateCommitmentAgreementCommandValidator : AbstractValidator<UpdateCommitmentAgreementCommand>
    {
        // REF: https://referencesource.microsoft.com/#System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs
        // This is the same Regex as used in the Employer User solution.
        private static readonly Regex emailRegEx = new Regex(
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(250));

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
