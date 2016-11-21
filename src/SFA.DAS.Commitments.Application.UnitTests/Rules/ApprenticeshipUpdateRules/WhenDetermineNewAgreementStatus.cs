using System;
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

        [TestCase(AgreementStatus.EmployerAgreed, CallerType.Provider, AgreementStatus.ProviderAgreed)]
        [TestCase(AgreementStatus.ProviderAgreed, CallerType.Employer, AgreementStatus.EmployerAgreed)]
        public void ThenSetToBothPartiesAgreedIfOtherPartyHasAlreadyAgreed(AgreementStatus currentAgreementStatus, CallerType caller, AgreementStatus partyAgreementStatus)
        {
            Assert.AreEqual(AgreementStatus.BothAgreed, _rules.DetermineNewAgreementStatus(currentAgreementStatus, caller, partyAgreementStatus));
        }

        [TestCase(CallerType.Employer, AgreementStatus.EmployerAgreed)]
        [TestCase(CallerType.Provider, AgreementStatus.ProviderAgreed)]
        public void ThenSetToCallingPartyAgreedIfCurrentlyNotAgreed(CallerType caller, AgreementStatus partyAgreementStatus)
        {
            Assert.AreEqual(partyAgreementStatus, _rules.DetermineNewAgreementStatus(AgreementStatus.NotAgreed, caller, partyAgreementStatus));
        }

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.Provider)]
        public void ThenSetToNotAgreedIfBothPartiesNotAgreed(CallerType caller)
        {
            Assert.AreEqual(AgreementStatus.NotAgreed, _rules.DetermineNewAgreementStatus(AgreementStatus.NotAgreed, caller, AgreementStatus.NotAgreed));
        }

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.Provider)]
        public void ThenShouldErrorIfTryingToSetToBothAgreedAndAlreadyAgreedByBothParties(CallerType caller)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rules.DetermineNewAgreementStatus(AgreementStatus.BothAgreed, caller, AgreementStatus.BothAgreed));
        }
    }
}
