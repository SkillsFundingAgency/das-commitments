using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Events.Api.Client;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsPublisherTests
{
    [TestFixture]
    public class WhenIPublishLargeNumbersOfEvents
    {
        private ApprenticeshipEventsPublisher _publisher;
        private Mock<IApprenticeshipEventsList> _eventsList;
        private Mock<IEventsApi> _eventsApi;


        [SetUp]
        public void Arrange()
        {
            _eventsList = new Mock<IApprenticeshipEventsList>();
            _eventsApi = new Mock<IEventsApi>();
            _publisher = new ApprenticeshipEventsPublisher(_eventsApi.Object, Mock.Of<ICommitmentsLogger>());

            _eventsApi.Setup(x =>
                    x.BulkCreateApprenticeshipEvent(It.IsAny<IList<Events.Api.Types.ApprenticeshipEvent>>()))
                .Returns(Task.CompletedTask);
        }

        private void SetupEvents(int eventCount)
        {
            var events = new List<IApprenticeshipEvent>();
            for (var i = 0; i < eventCount; i++)
            {
                events.Add(new ApprenticeshipEvent(new Commitment(), new Apprenticeship(), "TEST-EVENT", DateTime.UtcNow, null));
            }

            _eventsList.SetupGet(x => x.Events).Returns(events);
        }

        [TestCase(500, 1)]
        [TestCase(1000, 1)]
        [TestCase(1500, 2)]
        [TestCase(5000, 5)]
        [TestCase(15001, 16)]
        public async Task ThenTheyAreEmittedInBatches(int eventCount, int expectBatches)
        {
            //Arrange
            SetupEvents(eventCount);

            //Act
            await _publisher.Publish(_eventsList.Object);

            //Assert
            _eventsApi.Verify(x =>
                    x.BulkCreateApprenticeshipEvent(It.IsAny<IList<Events.Api.Types.ApprenticeshipEvent>>()), Times.Exactly(expectBatches));
        }
    }
}
