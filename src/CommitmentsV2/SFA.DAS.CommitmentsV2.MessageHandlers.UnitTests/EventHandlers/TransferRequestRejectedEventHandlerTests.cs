using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Fakes;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class TransferRequestRejectedEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingTransferRequestRejectedEvent_ThenShouldFindCohortAndResetCohortToBeWithEmployer()
        {
            var f = new TransferRequestRejectedEventHandlerTestsFixture().AddCohortToMemoryDb();
            await f.Handle();
            f.VerifyCohortIsWithEmployer();
        }

        [Test]
        public async Task Handle_WhenHandlingTransferRequestRejectedEvent_ThenShouldTrackingTheUpdate()
        {
            var f = new TransferRequestRejectedEventHandlerTestsFixture().AddCohortToMemoryDb();
            await f.Handle();
            f.VerifyEntityIsBeingTracked();
        }


        [Test]
        public async Task Handle_WhenHandlingTransferRequestRejectedEvent_ThenPublishesLegacyEventCohortRejectedByTransferSender()
        {
            var f = new TransferRequestRejectedEventHandlerTestsFixture().AddCohortToMemoryDb();
            await f.Handle();
            f.VerifyLegacyEventCohortRejectedByTransferSenderIsPublished();
        }

        [Test]
        public void Handle_WhenHandlingTransferRequestRejectedEventAndCohortIsNotFoundItThrowsException_ThenLogErrorAndRethrowError()
        {
            var f = new TransferRequestRejectedEventHandlerTestsFixture();
            Assert.ThrowsAsync<InvalidOperationException>(() => f.Handle());
            Assert.IsTrue(f.Logger.HasErrors);
        }

        [Test]
        public void Handle_WhenHandlingTransferRequestRejectedEventAndCohortIsNotWithTransferSenderItThrowsException_ThenLogErrorAndRethrowError()
        {
            var f = new TransferRequestRejectedEventHandlerTestsFixture().WithEmployerParty().AddCohortToMemoryDb();
            Assert.ThrowsAsync<DomainException>(() => f.Handle());
            Assert.IsTrue(f.Logger.HasErrors);
        }
    }

    public class TransferRequestRejectedEventHandlerTestsFixture
    {
        private Fixture _fixture;
        public FakeLogger<TransferRequestRejectedEvent> Logger { get; set; }
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher { get; set; }

        public ProviderCommitmentsDbContext Db { get; set; }
        public Cohort Cohort { get; set; }
        public DraftApprenticeship ExistingApprenticeshipDetails;
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public TransferRequestRejectedEvent TransferRequestRejectedEvent { get; set; } 
        public TransferRequestRejectedEventHandler Handler { get; set; } 

        public TransferRequestRejectedEventHandlerTestsFixture()
        {
            _fixture = new Fixture();
            UnitOfWorkContext = new UnitOfWorkContext();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            TransferRequestRejectedEvent = _fixture.Create<TransferRequestRejectedEvent>();

            Logger = new FakeLogger<TransferRequestRejectedEvent>();
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();
            Handler = new TransferRequestRejectedEventHandler(new Lazy<ProviderCommitmentsDbContext>(()=>Db), LegacyTopicMessagePublisher.Object, Logger);

            Cohort = new Cohort(
                    _fixture.Create<long>(),
                    _fixture.Create<long>(),
                    _fixture.Create<long>(),
                    null,
                    null,
                    Party.Employer,
                    "",
                    new UserInfo())
                { Id = TransferRequestRejectedEvent.CohortId, EmployerAccountId = 100, TransferSenderId = 99 };

            ExistingApprenticeshipDetails = new DraftApprenticeship(_fixture.Build<DraftApprenticeshipDetails>().Create(), Party.Provider);
            Cohort.Apprenticeships.Add(ExistingApprenticeshipDetails);
            Cohort.WithParty = Party.TransferSender;
            Cohort.TransferApprovalStatus = TransferApprovalStatus.Pending;
        }

        public Task Handle()
        {
            return Handler.Handle(TransferRequestRejectedEvent, Mock.Of<IMessageHandlerContext>());
        }

        public TransferRequestRejectedEventHandlerTestsFixture AddCohortToMemoryDb()
        {
            Db.Cohorts.Add(Cohort);
            Db.SaveChanges();

            return this;
        }

        public TransferRequestRejectedEventHandlerTestsFixture WithEmployerParty()
        {
            Cohort.WithParty = Party.Employer;
            return this;
        }

        public void VerifyCohortIsWithEmployer()
        {
            Assert.AreEqual(Party.Employer, Cohort.WithParty);
        }

        public void VerifyLegacyEventCohortRejectedByTransferSenderIsPublished()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<CohortRejectedByTransferSender>(p =>
                p.TransferRequestId == TransferRequestRejectedEvent.TransferRequestId &&
                p.ReceivingEmployerAccountId == Cohort.EmployerAccountId &&
                p.CommitmentId == Cohort.Id &&
                p.SendingEmployerAccountId == Cohort.TransferSenderId &&
                p.UserName == TransferRequestRejectedEvent.UserInfo.UserDisplayName &&
                p.UserEmail == TransferRequestRejectedEvent.UserInfo.UserEmail)));
        }

        public void VerifyEntityIsBeingTracked()
        {
            var list = UnitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().Where(x => x.StateChangeType == UserAction.RejectTransferRequest).ToList();

            Assert.AreEqual(1, list.Count);

            Assert.AreEqual(UserAction.RejectTransferRequest, list[0].StateChangeType);
            Assert.AreEqual(Cohort.Id, list[0].EntityId);
            Assert.AreEqual(TransferRequestRejectedEvent.UserInfo.UserDisplayName, list[0].UpdatingUserName);
            Assert.AreEqual(Party.TransferSender, list[0].UpdatingParty);
        }

    }
}