using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Commitments.Application.UnitTests.Services
{
    [TestFixture]
    public class V2EventsPublisherTests
    {
        public void Constructor_Valid_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<object>();

            var publisher = fixtures.CreateV2EventsPublisher();
        }

        #region PublishApprenticeshipDeleted
        [Test]
        public async Task PublishApprenticeshipDeleted_NoDatesSet_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<DraftApprenticeshipDeletedEvent>();

            await fixtures.Publish(publisher => publisher.PublishApprenticeshipDeleted(fixtures.Commitment, fixtures.Apprenticeship));
       }
        #endregion

        #region PublishApprenticeshipStopped
        [Test]
        public async Task PublishApprenticeshipStopped_WithStartAndEndDateSet_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipStoppedEvent>()
                .WithStopDate();

            await fixtures.Publish(publisher => publisher.PublishApprenticeshipStopped(fixtures.Commitment, fixtures.Apprenticeship));
        }

        [Test]
        public void PublishApprenticeshipCreated_WithoutStopDateSet_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipStoppedEvent>();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.Publish(publisher => publisher.PublishApprenticeshipStopped(fixtures.Commitment, fixtures.Apprenticeship)));
        }
        #endregion
        #region PublishApprenticeshipUpdateApproved
        [Test]
        public async Task PublishApprenticeshipUpdatedApproved_WithStartAndEndDateSet_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipUpdatedApprovedEvent>()
                .WithStartDate()
                .WithEndDate();

            await fixtures.Publish(publisher => publisher.PublishApprenticeshipUpdatedApproved(fixtures.Commitment, fixtures.Apprenticeship));
        }

        [Test]
        public void PublishApprenticeshipUpdatedApproved_WithoutStartDateSet_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipUpdatedApprovedEvent>()
                .WithEndDate();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.Publish(publisher => publisher.PublishApprenticeshipUpdatedApproved(fixtures.Commitment, fixtures.Apprenticeship)));
        }

        [Test]
        public void PublishApprenticeshipUpdatedApproved_WithoutEndDateSet_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipUpdatedApprovedEvent>()
                .WithStartDate();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.Publish(publisher => publisher.PublishApprenticeshipUpdatedApproved(fixtures.Commitment, fixtures.Apprenticeship)));
        }
        #endregion

        #region PublishDataLockTriageApproved
        [Test]
        public async Task PublishDataLockTriageApproved_WithStartAndEndDateSet_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<DataLockTriageApprovedEvent>()
                .WithStartDate()
                .WithEndDate();

            await fixtures.Publish(publisher => publisher.PublishDataLockTriageApproved(fixtures.ApprenticeshipEvent));
        }
        #endregion

        #region PublishApprenticeshipCreated
        [Test]
        public async Task PublishApprenticeshipCreated_WithStartAndEndDateSet_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipCreatedEvent>()
                .WithStartDate()
                .WithEndDate();

            await fixtures.Publish(publisher => publisher.PublishApprenticeshipCreated(fixtures.ApprenticeshipEvent));
        }

        [Test]
        public void PublishApprenticeshipCreated_WithoutStartDateSet_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipCreatedEvent>()
                .WithEndDate();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.Publish(publisher => publisher.PublishApprenticeshipCreated(fixtures.ApprenticeshipEvent)));
        }

        [Test]
        public void PublishApprenticeshipCreated_WithoutEndDateSet_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipCreatedEvent>()
                .WithStartDate();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.Publish(publisher => publisher.PublishApprenticeshipCreated(fixtures.ApprenticeshipEvent)));
        }
        #endregion
    }

    internal class V2EventsPublisherTestFixtures<TEvent> where TEvent : class
    {
        public V2EventsPublisherTestFixtures()
        {
            EndpointInstanceMock = new Mock<IEndpointInstance>();
            CommitmentsLoggerMock = new Mock<ICommitmentsLogger>();
            CurrentDateTimeMock = new Mock<ICurrentDateTime>();

            Apprenticeship = new Apprenticeship();
            Commitment = new Commitment();
            var apprenticeship = new Mock<IApprenticeshipEvent>();
            apprenticeship.Setup(a => a.Apprenticeship).Returns(Apprenticeship);
            apprenticeship.Setup(a => a.Commitment).Returns(Commitment);
            ApprenticeshipEvent = apprenticeship.Object;
        }

        public Mock<IEndpointInstance> EndpointInstanceMock { get; }

        public IEndpointInstance EndpointInstance => EndpointInstanceMock.Object;

        public Mock<ICommitmentsLogger> CommitmentsLoggerMock { get; }

        public ICommitmentsLogger CommitmentsLogger => CommitmentsLoggerMock.Object;

        public Mock<ICurrentDateTime> CurrentDateTimeMock { get; }

        public ICurrentDateTime CurrentDateTime => CurrentDateTimeMock.Object;

        public V2EventsPublisher CreateV2EventsPublisher()
        {
            return new V2EventsPublisher(EndpointInstance, CommitmentsLogger, CurrentDateTime);
        }

        public V2EventsPublisherTestFixtures<TEvent> WithStartDate()
        {
            Apprenticeship.StartDate = DateTime.Now;
            return this;
        }

        public V2EventsPublisherTestFixtures<TEvent> WithEndDate()
        {
            Apprenticeship.EndDate = DateTime.Now;
            return this;
        }

        public V2EventsPublisherTestFixtures<TEvent> WithStopDate()
        {
            Apprenticeship.StopDate = DateTime.Now;
            return this;
        }

        public Apprenticeship Apprenticeship { get; }

        public Commitment Commitment { get; }

        public IApprenticeshipEvent ApprenticeshipEvent { get; }

        public async Task<V2EventsPublisherTestFixtures<TEvent>> Publish(Func<IV2EventsPublisher, Task> publisherMethod)
        {
            SetUpMessageBuilder();

            var publisher = CreateV2EventsPublisher();

            await publisherMethod(publisher);

            return this;
        }

        public async Task<V2EventsPublisherTestFixtures<TEvent>> PublishApprenticeshipStopped()
        {
            var publisher = CreateV2EventsPublisher();

            await publisher.PublishApprenticeshipStopped(Commitment, Apprenticeship);

            return this;
        }

        public async Task<V2EventsPublisherTestFixtures<TEvent>> PublishApprenticeshipCreated()
        {
            var publisher = CreateV2EventsPublisher();

            await publisher.PublishApprenticeshipCreated(ApprenticeshipEvent);

            return this;
        }

        public async Task<V2EventsPublisherTestFixtures<TEvent>> PublishDataLockTriageApproved()
        {
            var publisher = CreateV2EventsPublisher();

            await publisher.PublishDataLockTriageApproved(ApprenticeshipEvent);

            return this;
        }

        public async Task<V2EventsPublisherTestFixtures<TEvent>> PublishApprenticeshipUpdatedApproved()
        {
            var publisher = CreateV2EventsPublisher();

            await publisher.PublishDataLockTriageApproved(ApprenticeshipEvent);

            return this;
        }

        private V2EventsPublisherTestFixtures<TEvent> SetUpMessageBuilder() 
        {
            EndpointInstanceMock
                .Setup(epi => epi.Publish<TEvent>(It.IsAny<Action<TEvent>>(), It.IsAny<PublishOptions>()))
                .Callback<Action<TEvent>, PublishOptions>((messageBuilder, options) => CallMessageBuilder(messageBuilder))
                .Returns(Task.CompletedTask);

            return this;
        }

        private void CallMessageBuilder(Action<TEvent> messageBuilder)
        {
            var messageInstance = new Mock<TEvent>();

            messageBuilder(messageInstance.Object);
        }
    }
}
