using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;


namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.StateTracking
{
    [TestFixture]
    public class WhenTrackingState
    {
        [Test]
        public void LastMessageReflectsTheLastAddedMessage()
        {
            const string testMessage = "TestMessage";
            var cohort = new CommitmentsV2.Models.Cohort();
            cohort.Messages.Add(new CommitmentsV2.Models.Message { Text = testMessage });
            Assert.That(cohort.LastMessage, Is.EqualTo(testMessage));
        }

        [Test]
        public void LastMessageIsNullWhenNoMessagesExist()
        {
            var cohort = new CommitmentsV2.Models.Cohort();
            Assert.That(cohort.LastMessage, Is.EqualTo(null));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void DraftApprenticeshipCountReflectsTheNumberOfDraftApprenticeships(int count)
        {
            var cohort = new CommitmentsV2.Models.Cohort();
            for (var i = 0; i < count; i++)
            {
                cohort.Apprenticeships.Add(new DraftApprenticeship());
            }

            Assert.That(cohort.DraftApprenticeshipCount, Is.EqualTo(count));
        }
    }
}
