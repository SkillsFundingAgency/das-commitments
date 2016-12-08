using System;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Rules.ApprenticeshipUpdateRules
{
    [TestFixture]
    public class WhenDetermineNewEditStatus
    {
        private Application.Rules.ApprenticeshipUpdateRules _rules;

        [SetUp]
        public void Setup()
        {
            _rules = new Application.Rules.ApprenticeshipUpdateRules();
        }

        [TestCase(EditStatus.EmployerOnly, CallerType.Employer)]
        [TestCase(EditStatus.Both, CallerType.Employer)]
        [TestCase(EditStatus.ProviderOnly, CallerType.Provider)]
        [TestCase(EditStatus.Both, CallerType.Provider)]
        public void ThenSetToBothPartiesCanEditIfNoApprenticeshipsArePendingAgreement(EditStatus existingEditStatus, CallerType caller)
        {
            Assert.AreEqual(EditStatus.Both, _rules.DetermineNewEditStatus(existingEditStatus, caller, false, 10));
        }

        [TestCase(CallerType.Employer, EditStatus.ProviderOnly)]
        [TestCase(CallerType.Provider, EditStatus.EmployerOnly)]
        public void ThenSetToOtherPartyCanEditIfApprenticeshipsArePendingAgreement(CallerType caller, EditStatus expectedEditStatus)
        {
            Assert.AreEqual(expectedEditStatus, _rules.DetermineNewEditStatus(EditStatus.Both, caller, true, 10));
        }

        [TestCase(EditStatus.EmployerOnly, CallerType.Employer, EditStatus.ProviderOnly)]
        [TestCase(EditStatus.ProviderOnly, CallerType.Provider, EditStatus.EmployerOnly)]
        public void ThenSetToOtherPartyCanEditIfThereAreNoApprenticeshipsInTheCommitment(EditStatus existingEditStatus, CallerType caller, EditStatus expectedStatus)
        {
            Assert.AreEqual(expectedStatus, _rules.DetermineNewEditStatus(existingEditStatus, caller, false, 0));
        }
    }
}
