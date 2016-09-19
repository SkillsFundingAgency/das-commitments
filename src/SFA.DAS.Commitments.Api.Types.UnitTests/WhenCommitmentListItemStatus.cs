using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Api.Types.UnitTests
{
    [TestFixture]
    public sealed class WhenCommitmentListItemStatus
    {
        [Test]
        public void IsReadyForApprovalItCanBeApproved()
        {
            var apprenticeship = new Apprenticeship { Status = ApprenticeshipStatus.ReadyForApproval };

            apprenticeship.CanBeApproved().Should().Be(true);
        }

        [TestCase(ApprenticeshipStatus.Approved)]
        [TestCase(ApprenticeshipStatus.Created)]
        public void IsNotReadyForApprovalItCannotBeApproved(ApprenticeshipStatus status)
        {
            var apprenticeship = new Apprenticeship { Status = status };

            apprenticeship.CanBeApproved().Should().Be(false);
        }
    }
}
