using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
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
        public FakeLogger<TransferRequestApprovedEventHandler> Logger { get; set; }
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

            Logger = new FakeLogger<TransferRequestApprovedEventHandler>();
            Handler = new TransferRequestApprovedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), Logger);

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
    }
}