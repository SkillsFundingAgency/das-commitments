﻿using System;
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
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class TransferRequestApprovedEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingTransferRequestApprovedEvent_ThenShouldFindCohortAndSetTransferApprovalProperties()
        {
            var f = new TransferRequestApprovedEventHandlerTestsFixture().AddCohortToMemoryDb();
            await f.Handle();
            f.VerifyCohortApprovalPropertiesAreSet();
        }

        [Test]
        public async Task Handle_WhenHandlingTransferRequestApprovedEvent_ThenShouldSendLegacyEventCohortApprovedByTransferSender()
        {
            var f = new TransferRequestApprovedEventHandlerTestsFixture().AddCohortToMemoryDb();
            await f.Handle();
            f.VerifyLegacyEventCohortApprovedByTransferSenderIsPublished();
        }

        [Test]
        public void Handle_WhenHandlingTransferRequestApprovedEventAndItThrowsException_ThenWelogErrorAndRethrowError()
        {
            var f = new TransferRequestApprovedEventHandlerTestsFixture();
            Assert.ThrowsAsync<InvalidOperationException>(() => f.Handle());
            Assert.IsTrue(f.Logger.HasErrors);
        }
    }

    public class TransferRequestApprovedEventHandlerTestsFixture
    {
        private Fixture _fixture;
        public FakeLogger<TransferRequestApprovedEvent> Logger { get; set; }
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher { get; set; }
        public UserInfo TransferSenderUserInfo { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public Cohort Cohort { get; set; }
        public DraftApprenticeship ExistingApprenticeshipDetails;
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public TransferRequestApprovedEvent TransferRequestApprovedEvent { get; set; } 
        public TransferRequestApprovedEventHandler Handler { get; set; } 

        public TransferRequestApprovedEventHandlerTestsFixture()
        {
            _fixture = new Fixture();
            UnitOfWorkContext = new UnitOfWorkContext();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            TransferSenderUserInfo = _fixture.Create<UserInfo>();
            TransferRequestApprovedEvent = new TransferRequestApprovedEvent(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), TransferSenderUserInfo);

            Logger = new FakeLogger<TransferRequestApprovedEvent>();
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();
            Handler = new TransferRequestApprovedEventHandler(new Lazy<ProviderCommitmentsDbContext>(()=>Db), LegacyTopicMessagePublisher.Object, Logger);

            Cohort = new Cohort(
                    new Provider(),
                    new AccountLegalEntity(),
                    null,
                    Party.Employer,
                    "",
                    new UserInfo())
                { Id = TransferRequestApprovedEvent.CohortId, EmployerAccountId = 100, TransferSenderId = 99 };

            ExistingApprenticeshipDetails = new DraftApprenticeship(_fixture.Build<DraftApprenticeshipDetails>().Create(), Party.Provider);
            Cohort.Apprenticeships.Add(ExistingApprenticeshipDetails);
            Cohort.EditStatus = EditStatus.Both;
            Cohort.TransferApprovalStatus = TransferApprovalStatus.Pending;
        }

        public Task Handle()
        {
            return Handler.Handle(TransferRequestApprovedEvent, Mock.Of<IMessageHandlerContext>());
        }

        public TransferRequestApprovedEventHandlerTestsFixture AddCohortToMemoryDb()
        {
            Db.Cohorts.Add(Cohort);
            Db.SaveChanges();

            return this;
        }
        public void VerifyCohortApprovalPropertiesAreSet()
        {
            Assert.AreEqual(Cohort.TransferApprovalStatus, TransferApprovalStatus.Approved);
            Assert.AreEqual(Cohort.TransferApprovalActionedOn, TransferRequestApprovedEvent.ApprovedOn);
        }

        public void VerifyLegacyEventCohortApprovedByTransferSenderIsPublished()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<CohortApprovedByTransferSender>(p =>
                p.TransferRequestId == TransferRequestApprovedEvent.TransferRequestId &&
                p.ReceivingEmployerAccountId == Cohort.EmployerAccountId &&
                p.CommitmentId == Cohort.Id &&
                p.SendingEmployerAccountId == Cohort.TransferSenderId &&
                p.UserEmail == TransferSenderUserInfo.UserEmail &&
                p.UserName == TransferSenderUserInfo.UserDisplayName)));
        }
    }
}