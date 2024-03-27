using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
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
    public class TransferRequestApprovedEventHandlerTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public async Task Handle_WhenHandlingTransferRequestApprovedEvent_ThenShouldFindCohortAndSetTransferApprovalProperties(bool autoApproval)
        {
            var fixture = new TransferRequestApprovedEventHandlerTestsFixture()
                .AddCohortToMemoryDb()
                .AddTransferRequest(autoApproval);

            await fixture.Handle();

            fixture.VerifyCohortApprovalPropertiesAreSet();
        }

        [Test]
        public async Task Handle_WhenHandlingTransferRequestApprovedEventAndAutoApprovalIsFalse_ThenShouldSendLegacyEventCohortApprovedByTransferSender()
        {
            var fixture = new TransferRequestApprovedEventHandlerTestsFixture()
                .AddCohortToMemoryDb()
                .AddTransferRequest(false);

            await fixture.Handle();

            fixture.VerifyLegacyEventCohortApprovedByTransferSenderIsPublished();
        }

        [Test]
        public async Task Handle_WhenHandlingTransferRequestApprovedEventAndAutoApprovalIsTrue_ThenShouldNotSendLegacyEventCohortApprovedByTransferSender()
        {
            var fixture = new TransferRequestApprovedEventHandlerTestsFixture()
                .AddCohortToMemoryDb()
                .AddTransferRequest(true);

            await fixture.Handle();

            fixture.VerifyMessageNotRelayed();
        }

        [Test]
        public void Handle_WhenHandlingTransferRequestApprovedEventAndItThrowsException_ThenWelogErrorAndRethrowError()
        {
            var fixture = new TransferRequestApprovedEventHandlerTestsFixture();

            Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handle());

            Assert.That(fixture.Logger.HasErrors, Is.True);
        }
    }

    public class TransferRequestApprovedEventHandlerTestsFixture
    {
        private readonly Fixture _fixture;
        public FakeLogger<TransferRequestApprovedEvent> Logger { get; set; }
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher { get; set; }
        public UserInfo TransferSenderUserInfo { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public TransferRequest TransferRequest { get; set; }
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
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            TransferSenderUserInfo = _fixture.Create<UserInfo>();
            TransferRequestApprovedEvent = new TransferRequestApprovedEvent(_fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<DateTime>(),
                TransferSenderUserInfo,
                _fixture.Create<int>(),
                _fixture.Create<decimal>(),
                _fixture.Create<int?>());

            Logger = new FakeLogger<TransferRequestApprovedEvent>();
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();
            Handler = new TransferRequestApprovedEventHandler(new Lazy<ProviderCommitmentsDbContext>(()=>Db), LegacyTopicMessagePublisher.Object, Logger);

            TransferRequest = new TransferRequest
                { Id = TransferRequestApprovedEvent.TransferRequestId, Status = TransferApprovalStatus.Pending, Cost = 1000, Cohort = Cohort };

            Cohort = new Cohort(
                    _fixture.Create<long>(),
                    _fixture.Create<long>(),
                    _fixture.Create<long>(),
                    null,
                    null,
                    Party.Employer,
                    "",
                    new UserInfo())
                { Id = TransferRequestApprovedEvent.CohortId, EmployerAccountId = 100, TransferSenderId = 99 };

            ExistingApprenticeshipDetails = new DraftApprenticeship(_fixture.Build<DraftApprenticeshipDetails>().Create(), Party.Provider);
            Cohort.Apprenticeships.Add(ExistingApprenticeshipDetails);
            Cohort.WithParty = Party.TransferSender;
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

        public TransferRequestApprovedEventHandlerTestsFixture AddTransferRequest(bool autoApproval)
        {
            TransferRequest.AutoApproval = autoApproval;

            Db.TransferRequests.Add(TransferRequest);
            Db.SaveChanges();

            return this;
        }

        public void VerifyCohortApprovalPropertiesAreSet()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Cohort.TransferApprovalStatus, Is.EqualTo(TransferApprovalStatus.Approved));
                Assert.That(TransferRequestApprovedEvent.ApprovedOn, Is.EqualTo(Cohort.TransferApprovalActionedOn));
            });
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

        public void VerifyMessageNotRelayed()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.IsAny<CohortApprovedByTransferSender>()),
                Times.Never);
        }
    }
}