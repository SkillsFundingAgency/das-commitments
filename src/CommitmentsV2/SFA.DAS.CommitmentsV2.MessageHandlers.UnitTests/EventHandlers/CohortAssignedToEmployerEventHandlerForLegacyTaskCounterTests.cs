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
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
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
    public class CohortAssignedToEmployerEventHandlerForLegacyTaskCounterTests
    {
        [Test]
        public async Task Handle_WhenHandlingCohortAssignedToEmployerEventHandlerForLegacyTaskCounterWhenActionWasByProvider_ThenShouldEmitLegacyEventCohortApprovalRequestedByProvider()
        {
            var f = new CohortAssignedToEmployerEventHandlerForLegacyTaskCounterTestsFixture().AddCohortToMemoryDb().LastAssignedParty(Party.Provider);
            await f.Handle();
            f.VerifyLegacyEventCohortApprovalRequestedByProviderIsSent();
        }

        [TestCase(Party.TransferSender)]
        [TestCase(Party.Employer)]
        public async Task Handle_WhenHandlingCohortAssignedToEmployerEventHandlerForLegacyTaskCounterWhenActionWasByProvider_ThenShouldNotEmitLegacyEventWhenAssigningPartyIsNotProvider(Party assigningParty)
        {
            var f = new CohortAssignedToEmployerEventHandlerForLegacyTaskCounterTestsFixture().AddCohortToMemoryDb().LastAssignedParty(assigningParty);
            await f.Handle();
            f.VerifyLegacyEventCohortApprovalRequestedByProviderIsNotSent();
        }


        [Test]
        public void Handle_WhenHandlingCohortAssignedToEmployerEventHandlerForLegacyTaskCounterWhenActionWasByProvider_ThenShouldThrowException()
        {
            var f = new CohortAssignedToEmployerEventHandlerForLegacyTaskCounterTestsFixture().LastAssignedParty(Party.Provider);
            Assert.ThrowsAsync<InvalidOperationException>(() => f.Handle());
            Assert.IsTrue(f.Logger.HasErrors);
        }
    }

    public class CohortAssignedToEmployerEventHandlerForLegacyTaskCounterTestsFixture
    {
        private Fixture _fixture;
        public long CohortId { get; set; }
        public DateTime Now { get; set; }
        public FakeLogger<CohortAssignedToEmployerEventHandlerForLegacyTaskCounter> Logger { get; set; }
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public Cohort Cohort { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public CohortAssignedToEmployerEvent CohortAssignedToEmployerEvent { get; set; } 
        public CohortAssignedToEmployerEventHandlerForLegacyTaskCounter Handler { get; set; } 

        public CohortAssignedToEmployerEventHandlerForLegacyTaskCounterTestsFixture()
        {
            _fixture = new Fixture();
            UnitOfWorkContext = new UnitOfWorkContext();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);
            CohortId = _fixture.Create<long>();
            Now = DateTime.UtcNow;
            
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();
            Logger = new FakeLogger<CohortAssignedToEmployerEventHandlerForLegacyTaskCounter>();
            Handler = new CohortAssignedToEmployerEventHandlerForLegacyTaskCounter(new Lazy<ProviderCommitmentsDbContext>(()=>Db), LegacyTopicMessagePublisher.Object, Logger);

            Cohort = new Cohort(
                    new Provider(),
                    new AccountLegalEntity(),
                    null,
                    Party.Employer,
                    "",
                    new UserInfo())
                {Id = CohortId, EmployerAccountId = 100, TransferSenderId = 99};
            Cohort.WithParty = Party.Employer;
        }

        public Task Handle()
        {
            return Handler.Handle(CohortAssignedToEmployerEvent, Mock.Of<IMessageHandlerContext>());
        }

        public CohortAssignedToEmployerEventHandlerForLegacyTaskCounterTestsFixture AddCohortToMemoryDb()
        {
            Db.Cohorts.Add(Cohort);
            Db.SaveChanges();

            return this;
        }

        public CohortAssignedToEmployerEventHandlerForLegacyTaskCounterTestsFixture LastAssignedParty(Party party)
        {
            CohortAssignedToEmployerEvent = new CohortAssignedToEmployerEvent(CohortId, DateTime.Now, party);

            return this;
        }

        public void VerifyLegacyEventCohortApprovalRequestedByProviderIsSent()
        {
            LegacyTopicMessagePublisher.Verify(x=>x.PublishAsync(It.Is<CohortApprovalRequestedByProvider>(p=> p.AccountId == Cohort.EmployerAccountId && p.ProviderId == Cohort.ProviderId && p.CommitmentId == Cohort.Id)));
        }

        public void VerifyLegacyEventCohortApprovalRequestedByProviderIsNotSent()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.IsAny<CohortApprovalRequestedByProvider>()), Times.Never);
        }
    }
}