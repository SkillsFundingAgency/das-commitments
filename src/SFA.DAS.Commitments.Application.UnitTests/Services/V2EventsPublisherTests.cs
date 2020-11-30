using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.ApproveTransferRequest;
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
            Assert.Pass();
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

        #region PublishProviderRejectedChangeOfPartyCohort
        [Test]
        public async Task PublishProviderRejectedChangeOfPartyCohort_ShouldNotThrowException()
        {
            var fixtures = new V2EventsPublisherTestFixtures<BulkUploadIntoCohortCompletedEvent>();

            await fixtures.Publish(publisher => publisher.PublishBulkUploadIntoCohortCompleted(fixtures.Commitment.ProviderId.Value, fixtures.Commitment.Id, 2));
            Assert.Pass();
        }

        [Test]
        public async Task PublishProviderRejectedChangeOfPartyCohort_ShouldPublishEventWithMappedValues()
        {
            var fixtures = new V2EventsPublisherTestFixtures<ProviderRejectedChangeOfPartyRequestEvent>()
                .WithChangeOfPartyRequest();

            var apprenticeship = fixtures.Commitment.Apprenticeships.FirstOrDefault();

            var employerAccountId = fixtures.Commitment.EmployerAccountId;
            var employerName = fixtures.Commitment.LegalEntityName;
            var trainingProvider = fixtures.Commitment.ProviderName;
            var changeOfPartyRequestId = fixtures.Commitment.ChangeOfPartyRequestId;
            var apprenticeName = $"{apprenticeship.FirstName} {apprenticeship.LastName}";

            await fixtures.Publish(publisher => publisher.PublishProviderRejectedChangeOfPartyCohort(fixtures.Commitment));
            fixtures.EndpointInstanceMock.Verify(x => x.Publish(It.Is<ProviderRejectedChangeOfPartyRequestEvent>(p => p.EmployerAccountId == employerAccountId && 
                p.EmployerName == employerName && 
                p.TrainingProviderName == trainingProvider && 
                p.ChangeOfPartyRequestId == changeOfPartyRequestId &&
                p.ApprenticeName == apprenticeName), It.IsAny<PublishOptions>()));
        }
        #endregion

        #region SendApproveTransferRequestCommand
        [Test]
        public async Task ApproveTransferRequestCommandSent_ShouldMapPropertiesCorrectly()
        {
            var f = new V2EventsPublisherTestFixtures<SFA.DAS.CommitmentsV2.Messages.Commands.ApproveTransferRequestCommand>();

            await f.CreateV2EventsPublisher().SendApproveTransferRequestCommand(f.TransferRequestId, f.Now, f.UserInfo);

            f.VerfiyApproveTransferRequestCommandWasSent();
        }

        [Test]
        public void ApproveTransferRequestCommandSent_ShouldThrowExceptionAndLogTheError()
        {
            var f = new V2EventsPublisherTestFixtures<SFA.DAS.CommitmentsV2.Messages.Commands.ApproveTransferRequestCommand>().ThrowsExceptionWhenCallingSend();

            Assert.CatchAsync<Exception>( () => f.CreateV2EventsPublisher().SendApproveTransferRequestCommand(1, f.Now, f.UserInfo));

            f.VerfiyErrorwasLogged();
        }
        #endregion

        #region SendRejectTransferRequestCommand
        [Test]
        public async Task RejectTransferRequestCommandSent_ShouldMapPropertiesCorrectly()
        {
            var f = new V2EventsPublisherTestFixtures<SFA.DAS.CommitmentsV2.Messages.Commands.RejectTransferRequestCommand>();

            await f.CreateV2EventsPublisher().SendRejectTransferRequestCommand(f.TransferRequestId, f.Now, f.UserInfo);

            f.VerfiyRejectTransferRequestCommandWasSent();
        }

        [Test]
        public void RejectTransferRequestCommandSent_ShouldThrowExceptionAndLogTheError()
        {
            var f = new V2EventsPublisherTestFixtures<SFA.DAS.CommitmentsV2.Messages.Commands.RejectTransferRequestCommand>().ThrowsExceptionWhenCallingSend();

            Assert.CatchAsync<Exception>(() => f.CreateV2EventsPublisher().SendRejectTransferRequestCommand(1, f.Now, f.UserInfo));

            f.VerfiyErrorwasLogged();
        }
        #endregion
    }

    internal class V2EventsPublisherTestFixtures<TEvent> where TEvent : class
    {
        public V2EventsPublisherTestFixtures()
        {
            TransferRequestId = 9798;
            Now = DateTime.Now;
            UserInfo = new UserInfo();
            EndpointInstanceMock = new Mock<IEndpointInstance>();
            MessageSessionMock = EndpointInstanceMock.As<IMessageSession>();
            CommitmentsLoggerMock = new Mock<ICommitmentsLogger>();
            CurrentDateTimeMock = new Mock<ICurrentDateTime>();
            CurrentDateTimeMock.Setup(x => x.Now).Returns(Now);

            Apprenticeship = new Apprenticeship();
            Apprenticeship.AgreedOn = DateTime.Today.AddDays(-1);
            Apprenticeship.FirstName = "First";
            Apprenticeship.LastName = "Last";
            Commitment = new Commitment {ProviderId = 123, Id = 1, Apprenticeships = new List<Apprenticeship> { Apprenticeship } };

            var apprenticeship = new Mock<IApprenticeshipEvent>();
            apprenticeship.Setup(a => a.Apprenticeship).Returns(Apprenticeship);
            apprenticeship.Setup(a => a.Commitment).Returns(Commitment);
            ApprenticeshipEvent = apprenticeship.Object;

            PaymentOrder = new List<int> {100, 200};
        }

        public long TransferRequestId { get; }
        public DateTime Now { get; }
        public UserInfo UserInfo { get; }
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

        public V2EventsPublisherTestFixtures<TEvent> WithChangeOfPartyRequest()
        {
            Commitment.ChangeOfPartyRequestId = 1;
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

        public V2EventsPublisherTestFixtures<TEvent> ThrowsExceptionWhenCallingSend()
        {
            EndpointInstanceMock.Setup(x => x.Send(It.IsAny<object>(), It.IsAny<SendOptions>()))
                .ThrowsAsync(new InvalidCastException());
            
            return this;
        }

        public void VerfiyApproveTransferRequestCommandWasSent()
        {
            EndpointInstanceMock.Verify(x =>
                x.Send(It.Is<object>(o => o is SFA.DAS.CommitmentsV2.Messages.Commands.ApproveTransferRequestCommand),
                    It.IsAny<SendOptions>()));
            EndpointInstanceMock.Verify(x => x.Send(It.Is<object>(o =>
                    ((CommitmentsV2.Messages.Commands.ApproveTransferRequestCommand) o).TransferRequestId == TransferRequestId &&
                    ((CommitmentsV2.Messages.Commands.ApproveTransferRequestCommand) o).ApprovedOn == Now &&
                    ((CommitmentsV2.Messages.Commands.ApproveTransferRequestCommand) o).UserInfo == UserInfo),
                It.IsAny<SendOptions>()));
        }

        public void VerfiyRejectTransferRequestCommandWasSent()
        {
            EndpointInstanceMock.Verify(x =>
                x.Send(It.Is<object>(o => o is SFA.DAS.CommitmentsV2.Messages.Commands.RejectTransferRequestCommand),
                    It.IsAny<SendOptions>()));
            EndpointInstanceMock.Verify(x => x.Send(It.Is<object>(o =>
                    ((CommitmentsV2.Messages.Commands.RejectTransferRequestCommand)o).TransferRequestId == TransferRequestId &&
                    ((CommitmentsV2.Messages.Commands.RejectTransferRequestCommand)o).RejectedOn == Now &&
                    ((CommitmentsV2.Messages.Commands.RejectTransferRequestCommand)o).UserInfo == UserInfo),
                It.IsAny<SendOptions>()));
        }


        public void VerfiyErrorwasLogged()
        {
            CommitmentsLoggerMock.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<int?>()));
        }

        private V2EventsPublisherTestFixtures<TEvent> SetUpMessageBuilder() 
        {
            EndpointInstanceMock
                .Setup(epi => epi.Publish<TEvent>(It.IsAny<Action<TEvent>>(), It.IsAny<PublishOptions>()))
                .Callback<Action<TEvent>, PublishOptions>((command, options) => CallMessageBuilder(command))
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