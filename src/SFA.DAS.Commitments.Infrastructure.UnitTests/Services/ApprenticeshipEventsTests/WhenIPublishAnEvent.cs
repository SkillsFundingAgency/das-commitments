using System;
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

        [Test]
        public async Task AndTheEffectiveFromDateIsProvidedThenTheEventIsPublishedWithTheCorrectEffectiveFromDate()
        {
            Apprenticeship.TrainingType = TrainingType.Standard;
            var effectiveFrom = DateTime.Now.AddDays(-1);

            await Service.PublishEvent(Commitment, Apprenticeship, _event, effectiveFrom);

            VerifyEventWasPublished(_event, effectiveFrom);
        }

        [Test]
        public async Task AndTheApprenticeIsBeingPausedThenTheEventIsPublishedWithThePauseDateSetToTheEffectiveFromDate()
        {
            Apprenticeship.PaymentStatus = Domain.Entities.PaymentStatus.Paused;
            var effectiveFrom = DateTime.Now.AddDays(-1);           // the date we want
            Apprenticeship.PauseDate = DateTime.Now.AddMonths(-1);  // the date we don't want

            await Service.PublishEvent(Commitment, Apprenticeship, _event, effectiveFrom);

            CommitmentsLogger.Verify(x => x.Info($"Create apprenticeship event: {_event}", null, null, Commitment.Id, Apprenticeship.Id, null, null), Times.Once);
            EventsApi.Verify(x => x.CreateApprenticeshipEvent(It.Is<ApprenticeshipEvent>(e => e.PausedOnDate == effectiveFrom)), Times.Once);
        }

        [Test]
        public async Task AndTheApprenticeIsNotBeingPausedThenTheEventIsPublishedWithThePauseDateSetToTheApprenticeshipsPauseDate()
        {
            Apprenticeship.PaymentStatus = Domain.Entities.PaymentStatus.Withdrawn;
            var effectiveFrom = DateTime.Now.AddDays(-1);                                   // the date we don't want
            var expectedPauseDate = Apprenticeship.PauseDate = DateTime.Now.AddMonths(-1);  // the date we want

            await Service.PublishEvent(Commitment, Apprenticeship, _event, effectiveFrom);

            CommitmentsLogger.Verify(x => x.Info($"Create apprenticeship event: {_event}", null, null, Commitment.Id, Apprenticeship.Id, null, null), Times.Once);
            EventsApi.Verify(x => x.CreateApprenticeshipEvent(It.Is<ApprenticeshipEvent>(e => e.PausedOnDate == expectedPauseDate)), Times.Once);
        }

        [Test]
        public async Task AndTheApprenticeIsBeingStoppedThenTheEventIsPublishedWithTheStopDateSetToTheEffectiveFromDate()
        {
            Apprenticeship.PaymentStatus = Domain.Entities.PaymentStatus.Withdrawn;
            var effectiveFrom = DateTime.Now.AddDays(-1);           // the date we want
            Apprenticeship.StopDate = DateTime.Now.AddMonths(-1);   // the date we don't want

            await Service.PublishEvent(Commitment, Apprenticeship, _event, effectiveFrom);

            CommitmentsLogger.Verify(x => x.Info($"Create apprenticeship event: {_event}", null, null, Commitment.Id, Apprenticeship.Id, null, null), Times.Once);
            EventsApi.Verify(x => x.CreateApprenticeshipEvent(It.Is<ApprenticeshipEvent>(e => e.StoppedOnDate == effectiveFrom)), Times.Once);
        }

        [Test]
        public async Task AndTheApprenticeIsNotBeingStoppedThenTheEventIsPublishedWithTheStopDateSetToTheApprenticeshipsStopDate()
        {
            Apprenticeship.PaymentStatus = Domain.Entities.PaymentStatus.Paused;
            var effectiveFrom = DateTime.Now.AddDays(-1);                                   // the date we don't want
            var expectedStopDate = Apprenticeship.StopDate = DateTime.Now.AddMonths(-1);    // the date we want

            await Service.PublishEvent(Commitment, Apprenticeship, _event, effectiveFrom);

            CommitmentsLogger.Verify(x => x.Info($"Create apprenticeship event: {_event}", null, null, Commitment.Id, Apprenticeship.Id, null, null), Times.Once);
            EventsApi.Verify(x => x.CreateApprenticeshipEvent(It.Is<ApprenticeshipEvent>(e => e.StoppedOnDate == expectedStopDate)), Times.Once);
        }

        private void VerifyEventWasPublished(string @event, DateTime? effectiveFrom = null)
        {
            CommitmentsLogger.Verify(x => x.Info($"Create apprenticeship event: {@event}", null, null, Commitment.Id, Apprenticeship.Id, null, null), Times.Once);
            EventsApi.Verify(x => x.CreateApprenticeshipEvent(It.Is<ApprenticeshipEvent>(y => EventMatchesParameters(y, @event, effectiveFrom))), Times.Once);
        }

        private bool EventMatchesParameters(ApprenticeshipEvent apprenticeshipEvent, string @event, DateTime? effectiveFrom)
        {
            return EventMatchesParameters(apprenticeshipEvent, @event, (PaymentStatus)Apprenticeship.PaymentStatus) && apprenticeshipEvent.EffectiveFrom == effectiveFrom;
        }
    }
}
