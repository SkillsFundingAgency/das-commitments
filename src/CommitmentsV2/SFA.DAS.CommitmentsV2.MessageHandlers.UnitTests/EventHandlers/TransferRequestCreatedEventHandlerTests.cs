using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TransferRequestCreatedEventHandlerTests
    {

        [Test]
        public void
            Handle_WhenHandlingTransferRequestCreatedEvent_IfLastApprovedByPartyIsEmployer_ThenShouldRelayMessageToAzureServiceBus()
        {
            var fixture = new TransferRequestCreatedEventHandlerTestsFixture();
            fixture.SetupTransfer().SetupTransferCreatedEvent(Party.Employer);

            fixture.Handle();

            fixture.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }

        [Test]
        public void
            Handle_WhenHandlingTransferRequestCreatedEvent_IfLastApprovedByPartyIsEmployer_ThenShouldNoyRelayMessageToAzureServiceBus()
        {
            var fixture = new TransferRequestCreatedEventHandlerTestsFixture();
            fixture.SetupTransfer().SetupTransferCreatedEvent(Party.Provider);

            fixture.Handle();

            fixture.VerifyMessageNotRelayed();
        }
    }

    public class TransferRequestCreatedEventHandlerTestsFixture
    {
        public Mock<IMessageHandlerContext> MessageHandlerContext;
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher;
        public TransferRequestCreatedEventHandler Sut;
        public TransferRequestCreatedEvent TransferRequestCreatedEvent;
        public ProviderCommitmentsDbContext Db { get; set; }
        public Cohort Cohort { get; set; }
        public TransferRequest TransferRequest { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public TransferRequestCreatedEventHandlerTestsFixture()
        {
            UnitOfWorkContext = new UnitOfWorkContext();
            MessageHandlerContext = new Mock<IMessageHandlerContext>();
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            Sut = new TransferRequestCreatedEventHandler(LegacyTopicMessagePublisher.Object, Mock.Of<ILogger<TransferRequestCreatedEvent>>(), new Lazy<ProviderCommitmentsDbContext>(() => Db));

            Cohort = new Cohort(
                new Provider(),
                new AccountLegalEntity(),
                null,
                Party.Employer,
                "",
                new UserInfo()) {EmployerAccountId = 100, TransferSenderId = 99};

            TransferRequest = new TransferRequest
                { Status = (byte)TransferApprovalStatus.Pending, Cost = 1000, Cohort = Cohort};
        }

        public TransferRequestCreatedEventHandlerTestsFixture SetupTransferCreatedEvent(Party lastApprovedParty)
        {
            TransferRequestCreatedEvent = new TransferRequestCreatedEvent(TransferRequest.Id, TransferRequest.Cohort.Id, DateTime.Now, lastApprovedParty);

            return this;
        }

        public TransferRequestCreatedEventHandlerTestsFixture SetupTransfer()
        {
            Db.TransferRequests.Add(TransferRequest);
            Db.SaveChanges();

            return this;
        }

        public Task Handle()
        {
            return Sut.Handle(TransferRequestCreatedEvent, Mock.Of<IMessageHandlerContext>());
        }

        public void VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<CohortApprovalByTransferSenderRequested>(p =>
                p.TransferRequestId == TransferRequest.Id &&
                p.ReceivingEmployerAccountId == TransferRequest.Cohort.EmployerAccountId &&
                p.SendingEmployerAccountId == TransferRequest.Cohort.TransferSenderId.Value &&
                p.TransferCost == TransferRequest.Cost &&
                p.CommitmentId == TransferRequest.CommitmentId)));
        }

        public void VerifyMessageNotRelayed()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.IsAny<CohortApprovalByTransferSenderRequested>()),
                Times.Never);
        }
    }
}


