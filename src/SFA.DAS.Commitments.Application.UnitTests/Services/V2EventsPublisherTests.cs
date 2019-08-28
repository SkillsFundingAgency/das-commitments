using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using TransferApprovalStatus = SFA.DAS.Commitments.Domain.Entities.TransferApprovalStatus;

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

        #region PublishApprenticeshipPaused
        [Test]
        public async Task PublishApprenticeshipPaused_WithPauseDateSet_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipStoppedEvent>()
                .WithPauseDate();

            await fixtures.Publish(publisher => publisher.PublishApprenticeshipPaused(fixtures.Commitment, fixtures.Apprenticeship));
        }

        [Test]
        public void PublishApprenticeshipCreated_WithoutPauseDateSet_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipStoppedEvent>();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.Publish(publisher => publisher.PublishApprenticeshipPaused(fixtures.Commitment, fixtures.Apprenticeship)));
        }
        #endregion



        #region PublishApprenticeshipStopDateChangedEvent
        [Test]
        public async Task PublishApprenticeshipStopDateChangedEvent_WithAStopDate_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipStopDateChangedEvent>()
                .WithStopDate();

            await fixtures.PublishApprenticeshipStopDateChanged();
            
        }

        [Test]
        public void PublishApprenticeshipStopDateChangedEvent_WithoutStopDateSet_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipStopDateChangedEvent>();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.PublishApprenticeshipStopDateChanged());
        }
        #endregion

        #region PublishApprenticeshipResumed
        [Test]
        public async Task PublishApprenticeshipResumed_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipResumedEvent>();

            await fixtures.Publish(publisher => publisher.PublishApprenticeshipResumed(fixtures.Commitment, fixtures.Apprenticeship));
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

        [Test]
        public async Task PublishApprenticeshipCreated_WithoutATransferSender_ShouldSetAgreedOnSameAsCreatedOn()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipCreatedEvent>()
                .WithStartDate()
                .WithEndDate();

            await fixtures.Publish(publisher => publisher.PublishApprenticeshipCreated(fixtures.ApprenticeshipEvent));

            fixtures.EndpointInstanceMock.Verify(x=>x.Publish(It.Is<ApprenticeshipCreatedEvent>(p=>p.AgreedOn == p.CreatedOn), It.IsAny<PublishOptions>()));

        }

        [Test]
        public async Task PublishApprenticeshipCreated_WithATransferSender_ShouldSetAgreedOnDifferentToCreatedOn()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipCreatedEvent>()
                .WithStartDate()
                .WithEndDate()
                .WithTransferApprovalData();

            await fixtures.Publish(publisher => publisher.PublishApprenticeshipCreated(fixtures.ApprenticeshipEvent));

            fixtures.EndpointInstanceMock.Verify(x => x.Publish(It.Is<ApprenticeshipCreatedEvent>(p => p.AgreedOn != p.CreatedOn), It.IsAny<PublishOptions>()));
        }

        [TestCase(null)]
        [TestCase(ApprenticeshipEmployerType.NonLevy)]
        [TestCase(ApprenticeshipEmployerType.Levy)]
        public async Task PublishApprenticeshipCreated_ShouldSetApprenticeshipEmployerTypeOnApproval(ApprenticeshipEmployerType? apprenticeshipEmployerType)
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipCreatedEvent>()
                .WithStartDate()
                .WithEndDate()
                .WithApprenticeshipEmployerTypeOnApproval(apprenticeshipEmployerType);
            
            await fixtures.Publish(publisher => publisher.PublishApprenticeshipCreated(fixtures.ApprenticeshipEvent));

            fixtures.EndpointInstanceMock.Verify(x => x.Publish(It.Is<ApprenticeshipCreatedEvent>(p => p.ApprenticeshipEmployerTypeOnApproval == apprenticeshipEmployerType), It.IsAny<PublishOptions>())); 
        }
        #endregion

        #region PublishPaymentOrderChanged
        [Test]
        public async Task PublishPaymentOrderChanged_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipResumedEvent>();

            await fixtures.Publish(publisher => publisher.PublishPaymentOrderChanged(100, fixtures.PaymentOrder));
        }

        [Test]
        public void PublishPaymentOrderChanged_WithZeroEmployerAccountId_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipStoppedEvent>();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.Publish(publisher => publisher.PublishPaymentOrderChanged(0, fixtures.PaymentOrder)));
        }

        [Test]
        public void PublishPaymentOrderChanged_WithNullList_ShouldThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ApprenticeshipStoppedEvent>();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixtures.Publish(publisher => publisher.PublishPaymentOrderChanged(100, null)));
        }
        #endregion

        #region PublishBulkUploadIntoCohortCreated
        [Test]
        public async Task PublishBulkUploadIntoCohortCreatedEvent_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<BulkUploadIntoCohortCompletedEvent>();

            await fixtures.Publish(publisher => publisher.PublishBulkUploadIntoCohortCompleted(fixtures.Commitment.ProviderId.Value, fixtures.Commitment.Id, 2));
        }

        [Test]
        public async Task PublishBulkUploadIntoCohortCreatedEvent_ShouldPublishEventWithMappedValues()
        {
            var fixtures = new V2EventsPublisherTestFixtures<BulkUploadIntoCohortCompletedEvent>();
            var providerId = fixtures.Commitment.ProviderId.Value;
            var cohortId = fixtures.Commitment.Id;
            uint count = 2;

            await fixtures.Publish(publisher => publisher.PublishBulkUploadIntoCohortCompleted(providerId, cohortId, count));
            fixtures.EndpointInstanceMock.Verify(x=>x.Publish(It.Is<BulkUploadIntoCohortCompletedEvent>(p=>p.CohortId == cohortId && p.ProviderId == providerId && p.NumberOfApprentices == count && p.UploadedOn == fixtures.Now), It.IsAny<PublishOptions>()));
        }
        #endregion

    }

    internal class V2EventsPublisherTestFixtures<TEvent> where TEvent : class
    {
        public V2EventsPublisherTestFixtures()
        {
            Now = DateTime.Now;
            EndpointInstanceMock = new Mock<IEndpointInstance>();
            MessageSessionMock = EndpointInstanceMock.As<IMessageSession>();
            CommitmentsLoggerMock = new Mock<ICommitmentsLogger>();
            CurrentDateTimeMock = new Mock<ICurrentDateTime>();
            CurrentDateTimeMock.Setup(x => x.Now).Returns(Now);

            Apprenticeship = new Apprenticeship();
            Apprenticeship.AgreedOn = DateTime.Today.AddDays(-1);
            Commitment = new Commitment {ProviderId = 123, Id = 1};

            var apprenticeship = new Mock<IApprenticeshipEvent>();
            apprenticeship.Setup(a => a.Apprenticeship).Returns(Apprenticeship);
            apprenticeship.Setup(a => a.Commitment).Returns(Commitment);
            ApprenticeshipEvent = apprenticeship.Object;

            PaymentOrder = new List<int> {100, 200};
        }

        public DateTime Now { get; }
        public List<int> PaymentOrder { get; }
        public Mock<IEndpointInstance> EndpointInstanceMock { get; }
        public Mock<IMessageSession> MessageSessionMock { get; }
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

        public V2EventsPublisherTestFixtures<TEvent> WithPauseDate()
        {
            Apprenticeship.PauseDate = DateTime.Now;
            return this;
        }

        public V2EventsPublisherTestFixtures<TEvent> WithTransferApprovalData()
        {
            Commitment.TransferApprovalActionedOn = DateTime.Today;
            Commitment.TransferSenderId = 12344;
            Commitment.TransferApprovalStatus = TransferApprovalStatus.TransferApproved;
            return this;
        }

        public V2EventsPublisherTestFixtures<TEvent> WithApprenticeshipEmployerTypeOnApproval(ApprenticeshipEmployerType? apprenticeshipEmployerType)
        {
            Commitment.ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerType;
            
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

        public async Task<V2EventsPublisherTestFixtures<TEvent>> PublishApprenticeshipStopDateChanged()
        {
            var publisher = CreateV2EventsPublisher();

            await publisher.PublishApprenticeshipStopDateChanged(Commitment, Apprenticeship);

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