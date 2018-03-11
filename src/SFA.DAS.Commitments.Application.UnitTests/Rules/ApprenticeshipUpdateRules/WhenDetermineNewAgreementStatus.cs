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
        public void ThenSetAsNotAgreedIfChangesThatRequireAgreementWereMade(AgreementStatus agreementStatus)
        {
            Assert.AreEqual(AgreementStatus.NotAgreed, _rules.DetermineNewAgreementStatus(agreementStatus, CallerType.Employer, true));
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


        [TestCase(AgreementStatus.EmployerAgreed, CallerType.Provider)]
        [TestCase(AgreementStatus.ProviderAgreed, CallerType.Provider)]
        [TestCase(AgreementStatus.EmployerAgreed, CallerType.Employer)]
        [TestCase(AgreementStatus.ProviderAgreed, CallerType.Employer)]
        public void ThenIfNoActionWasTakenThenTheAgreementStatusIsNotChanged(AgreementStatus currentAgreementStatus, CallerType callerType)
        {
            //Act
            var result = _rules.DetermineNewAgreementStatus(currentAgreementStatus, callerType, LastAction.None);

            //Assert
            Assert.AreEqual(currentAgreementStatus, result);

        }
    }
}
