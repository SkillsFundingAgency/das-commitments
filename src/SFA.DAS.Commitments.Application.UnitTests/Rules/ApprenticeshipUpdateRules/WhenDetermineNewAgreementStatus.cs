using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Rules.ApprenticeshipUpdateRules
{
    [TestFixture]
    public class WhenDetermineNewAgreementStatus
    {
        private Application.Rules.ApprenticeshipUpdateRules _rules;

        [SetUp]
        public void Setup()
        {
            _rules = new Application.Rules.ApprenticeshipUpdateRules();
        }

        [TestCase(AgreementStatus.NotAgreed)]
        [TestCase(AgreementStatus.EmployerAgreed)]
        [TestCase(AgreementStatus.ProviderAgreed)]
        [TestCase(AgreementStatus.BothAgreed)]
        public void ThenLeaveAsIsIfNoChangesThatRequirementAgreementWereMade(AgreementStatus agreementStatus)
        {
            Assert.AreEqual(agreementStatus, _rules.DetermineNewAgreementStatus(agreementStatus, CallerType.Employer, false));
        }

        [TestCase(AgreementStatus.NotAgreed)]
        [TestCase(AgreementStatus.EmployerAgreed)]
        [TestCase(AgreementStatus.ProviderAgreed)]
        [TestCase(AgreementStatus.BothAgreed)]
        public void ThenSetToEmployerAgreedIfTheCallerIsTheEmployer(AgreementStatus employerAgreementStatus)
        {
            Assert.AreEqual(AgreementStatus.EmployerAgreed, _rules.DetermineNewAgreementStatus(employerAgreementStatus, CallerType.Employer, true));
        }

        [TestCase(AgreementStatus.NotAgreed)]
        [TestCase(AgreementStatus.EmployerAgreed)]
        [TestCase(AgreementStatus.ProviderAgreed)]
        [TestCase(AgreementStatus.BothAgreed)]
        public void ThenSetToProviderAgreedIfTheCallerIsTheProvider(AgreementStatus providerAgreementStatus)
        {
            Assert.AreEqual(AgreementStatus.ProviderAgreed, _rules.DetermineNewAgreementStatus(providerAgreementStatus, CallerType.Provider, true));
        }

        [TestCase(AgreementStatus.EmployerAgreed, CallerType.Provider, LastAction.Approve)]
        [TestCase(AgreementStatus.ProviderAgreed, CallerType.Employer, LastAction.Approve)]
        public void ThenSetToBothPartiesAgreedIfOtherPartyHasAlreadyAgreed(AgreementStatus currentAgreementStatus, CallerType caller, LastAction action)
        {
            Assert.AreEqual(AgreementStatus.BothAgreed, _rules.DetermineNewAgreementStatus(currentAgreementStatus, caller, action));
        }

        [TestCase(CallerType.Employer, LastAction.Approve, AgreementStatus.EmployerAgreed)]
        [TestCase(CallerType.Provider, LastAction.Approve, AgreementStatus.ProviderAgreed)]
        public void ThenSetToCallingPartyAgreedIfCurrentlyNotAgreed(CallerType caller, LastAction action, AgreementStatus expectedAgreementStatus)
        {
            Assert.AreEqual(expectedAgreementStatus, _rules.DetermineNewAgreementStatus(AgreementStatus.NotAgreed, caller, action));
        }

        [TestCase(AgreementStatus.ProviderAgreed,CallerType.Employer, LastAction.Amend)]
        [TestCase(AgreementStatus.EmployerAgreed, CallerType.Provider, LastAction.Amend)]
        public void ThenSetToToNotAgreedIfOtherPartyIsApprovedAndAmending(AgreementStatus currentAgreementStatus, CallerType caller, LastAction action)
        {
            Assert.AreEqual(AgreementStatus.NotAgreed, _rules.DetermineNewAgreementStatus(currentAgreementStatus, caller, action));
        }

        [TestCase(AgreementStatus.EmployerAgreed, CallerType.Employer, LastAction.Amend)]
        [TestCase(AgreementStatus.ProviderAgreed, CallerType.Provider, LastAction.Amend)]
        public void ThenLeaveAsCurrentStatusIfIsApprovedByPartyAndAmending(AgreementStatus currentAgreementStatus, CallerType caller, LastAction action)
        {
            Assert.AreEqual(currentAgreementStatus, _rules.DetermineNewAgreementStatus(currentAgreementStatus, caller, action));
        }

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.Provider)]
        public void ThenSetToNotAgreedIfBothPartiesNotAgreedAndAmending(CallerType caller)
        {
            Assert.AreEqual(AgreementStatus.NotAgreed, _rules.DetermineNewAgreementStatus(AgreementStatus.NotAgreed, caller, LastAction.Amend));
        }

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.Provider)]
        public void ThenShouldErrorIfTryingToApproveIfAlreadyAgreedByBothParties(CallerType caller)
        {
            Action act = () => _rules.DetermineNewAgreementStatus(AgreementStatus.BothAgreed, caller, LastAction.Approve);

            act.ShouldThrow<ArgumentException>()
                .WithMessage($"Invalid combination of values - CurrentAgreementStatus:{AgreementStatus.BothAgreed}, Caller:{caller}, Action:{LastAction.Approve}");
        }
    }
}
