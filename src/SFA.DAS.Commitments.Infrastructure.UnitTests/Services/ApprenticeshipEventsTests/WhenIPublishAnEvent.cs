using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Types;
using PaymentStatus = SFA.DAS.Events.Api.Types.PaymentStatus;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsTests
{
    [TestFixture]
    public class WhenIPublishAnEvent : ApprenticeshipEventsTestsBase
    {
        private string _event = "Test";

        [Test]
        public async Task AndTheTrainingTypeIsFrameworkThenTheEventIsPublishedWithTheCorrectTrainingType()
        {
            Apprenticeship.TrainingType = TrainingType.Framework;

            await Service.PublishEvent(Commitment, Apprenticeship, _event);

            VerifyEventWasPublished(_event);
        }

        [Test]
        public async Task AndTheTrainingTypeIsStandardThenTheEventIsPublishedWithTheCorrectTrainingType()
        {
            Apprenticeship.TrainingType = TrainingType.Standard;

            await Service.PublishEvent(Commitment, Apprenticeship, _event);

            VerifyEventWasPublished(_event);
        }
        
        private void VerifyEventWasPublished(string @event)
        {
            CommitmentsLogger.Verify(x => x.Info($"Create apprenticeship event: {@event}", null, null, Commitment.Id, Apprenticeship.Id), Times.Once);
            EventsApi.Verify(x => x.CreateApprenticeshipEvent(It.Is<ApprenticeshipEvent>(y => EventMatchesParameters(y, @event))), Times.Once);
        }

        private bool EventMatchesParameters(ApprenticeshipEvent apprenticeshipEvent, string @event)
        {
            return EventMatchesParameters(apprenticeshipEvent, @event, (PaymentStatus)Apprenticeship.PaymentStatus);
        }
    }
}
