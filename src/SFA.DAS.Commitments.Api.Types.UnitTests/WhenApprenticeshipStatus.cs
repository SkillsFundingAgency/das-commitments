using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Api.Types.UnitTests
{
    [TestFixture]
    public sealed class WhenApprenticeshipStatus
    {
        [Test]
        public void IsDraftCanBeSubmitted()
        {
            var commitment = new CommitmentListItem { Status = CommitmentStatus.Draft };

            commitment.CanBeSubmitted().Should().Be(true);
        }

        [Test]
        public void IsActiveCannotBeSubmitted()
        {
            var commitment = new CommitmentListItem { Status = CommitmentStatus.Active };

            commitment.CanBeSubmitted().Should().Be(false);
        }
    }
}
