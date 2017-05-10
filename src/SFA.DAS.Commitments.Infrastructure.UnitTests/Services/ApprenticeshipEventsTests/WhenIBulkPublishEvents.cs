using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Types;
using PaymentStatus = SFA.DAS.Events.Api.Types.PaymentStatus;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsTests
{
    [TestFixture]
    public class WhenIBulkPublishEvents : ApprenticeshipEventsTestsBase
    {
        private string _event = "Test";

        [Test]
        public async Task ThenTheEventsArePublished()
        {
            var apprenticeships = new List<Apprenticeship> { Apprenticeship, Apprenticeship };
            await Service.BulkPublishEvent(Commitment, apprenticeships, _event);

            VerifyEventWasPublished(_event);
        }

        [Test]
        public async Task AndThereAreNoApprenticeshipsThenEventsAreNotPublished()
        {
            var apprenticeships = new List<Apprenticeship>();

            await Service.BulkPublishEvent(Commitment, apprenticeships, _event);

            VerifyEventWasNotPublished();
        }

        private void VerifyEventWasNotPublished()
        {
            EventsApi.Verify(x => x.BulkCreateApprenticeshipEvent(It.IsAny<List<ApprenticeshipEvent>>()), Times.Never);
        }

        private void VerifyEventWasPublished(string @event)
        {
            CommitmentsLogger.Verify(x => x.Info("Creating apprenticeship events", null, null, null, null, null), Times.Once);
            EventsApi.Verify(x => x.BulkCreateApprenticeshipEvent(It.Is<List<ApprenticeshipEvent>>(y => y.TrueForAll(z => EventMatchesParameters(z, @event)))), Times.Once);
        }

        private bool EventMatchesParameters(ApprenticeshipEvent apprenticeshipEvent, string @event)
        {
            return EventMatchesParameters(apprenticeshipEvent, @event, (PaymentStatus)Apprenticeship.PaymentStatus);
        }
    }
}
