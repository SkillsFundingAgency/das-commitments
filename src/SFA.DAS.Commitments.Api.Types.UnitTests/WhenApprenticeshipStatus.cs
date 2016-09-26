using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Api.Types.UnitTests
{
    [TestFixture]
    public sealed class WhenApprenticeshipStatus
    {
        [Test]
        public void IsReadyForApprovalItCanBeApproved()
        {
            var apprenticeship = new Apprenticeship { Status = ApprenticeshipStatus.ReadyForApproval };

            apprenticeship.CanBeApproved().Should().Be(true);
        }

        [TestCase(ApprenticeshipStatus.Approved)]
        [TestCase(ApprenticeshipStatus.Paused)]
        public void IsNotReadyForApprovalItCannotBeApproved(ApprenticeshipStatus status)
        {
            var apprenticeship = new Apprenticeship { Status = status };

            apprenticeship.CanBeApproved().Should().Be(false);
        }

        [Test]
        public void IsApprovedItCanBePaused()
        {
            var apprenticeship = new Apprenticeship { Status = ApprenticeshipStatus.Approved };

            apprenticeship.CanBePaused().Should().Be(true);
        }

        [TestCase(ApprenticeshipStatus.Paused)]
        [TestCase(ApprenticeshipStatus.ReadyForApproval)]
        public void IsNotApprovedItCannotBePaused(ApprenticeshipStatus status)
        {
            var apprenticeship = new Apprenticeship { Status = status };

            apprenticeship.CanBePaused().Should().Be(false);
        }

        [Test]
        public void IsPausedItCanBeResumed()
        {
            var apprenticeship = new Apprenticeship { Status = ApprenticeshipStatus.Paused };

            apprenticeship.CanBeResumed().Should().Be(true);
        }

        [TestCase(ApprenticeshipStatus.Approved)]
        [TestCase(ApprenticeshipStatus.ReadyForApproval)]
        public void IsNotPausedItCannotBePaused(ApprenticeshipStatus status)
        {
            var apprenticeship = new Apprenticeship { Status = status };

            apprenticeship.CanBeResumed().Should().Be(false);
        }
    }
}
