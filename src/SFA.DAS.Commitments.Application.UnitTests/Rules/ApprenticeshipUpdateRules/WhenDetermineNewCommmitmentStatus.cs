using System;
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

        [Test, Ignore("Awaiting change after private beta wave 2b")]
        public void ThenSetToDeletedIfThereAreNoApprenticeshipsPendingAgreement()
        {
            Assert.AreEqual(CommitmentStatus.Deleted, _rules.DetermineNewCommmitmentStatus(false));
        }

        [Test]
        public void ThenSetToActiveIfThereAreApprenticeshipsPendingAgreement()
        {
            Assert.AreEqual(CommitmentStatus.Active, _rules.DetermineNewCommmitmentStatus(true));
        }
    }
}
