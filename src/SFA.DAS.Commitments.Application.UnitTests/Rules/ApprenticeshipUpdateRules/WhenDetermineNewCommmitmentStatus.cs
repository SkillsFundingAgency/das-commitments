using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Rules.ApprenticeshipUpdateRules
{
    [TestFixture]
    public class WhenDetermineNewCommmitmentStatus
    {
        private Application.Rules.ApprenticeshipUpdateRules _rules;

        [SetUp]
        public void Setup()
        {
            _rules = new Application.Rules.ApprenticeshipUpdateRules();
        }

        [Test]
        public void ThenSetToActiveIfThereAreApprenticeshipsPendingAgreement()
        {
            Assert.AreEqual(CommitmentStatus.Active, _rules.DetermineNewCommmitmentStatus(true));
        }
    }
}
