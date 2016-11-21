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

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.Provider)]
        public void ThenSetToBothPartiesCanEditIfNoApprenticeshipsArePendingAgreement(CallerType caller)
        {
            Assert.AreEqual(EditStatus.Both, _rules.DetermineNewEditStatus(caller, false));
        }

        [TestCase(CallerType.Employer, EditStatus.ProviderOnly)]
        [TestCase(CallerType.Provider, EditStatus.EmployerOnly)]
        public void ThenSetToOtherPartyCanEditIfApprenticeshipsArePendingAgreement(CallerType caller, EditStatus expectedEditStatus)
        {
            Assert.AreEqual(expectedEditStatus, _rules.DetermineNewEditStatus(caller, true));
        }
    }
}
