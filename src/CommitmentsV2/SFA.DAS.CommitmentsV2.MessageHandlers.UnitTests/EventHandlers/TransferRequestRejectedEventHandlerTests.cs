using System.Linq;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
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
        [TestCase(true)]
        [TestCase(false)]
        public async Task Handle_WhenHandlingTransferRequestRejectedEvent_ThenShouldFindCohortAndResetCohortToBeWithEmployer(bool autoApprove)
        {
            using var fixture = new TransferRequestRejectedEventHandlerTestsFixture()
                .AddCohortToMemoryDb()
                .AddTransferRequest(autoApprove);

            await fixture.Handle();

            fixture.VerifyCohortIsWithEmployer();
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Handle_WhenHandlingTransferRequestRejectedEvent_ThenShouldTrackingTheUpdate(bool autoApprove)
        {
            using var fixture = new TransferRequestRejectedEventHandlerTestsFixture()
                .AddCohortToMemoryDb()
                .AddTransferRequest(autoApprove);

            await fixture.Handle();

            fixture.VerifyEntityIsBeingTracked();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Handle_WhenHandlingTransferRequestRejectedEventAndCohortIsNotFoundItThrowsException_ThenLogErrorAndRethrowError(bool autoApprove)
        {
            using var fixture = new TransferRequestRejectedEventHandlerTestsFixture()
                .AddTransferRequest(autoApprove);

            Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handle());

            Assert.That(fixture.Logger.HasErrors, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Handle_WhenHandlingTransferRequestRejectedEventAndCohortIsNotWithTransferSenderItThrowsException_ThenLogErrorAndRethrowError(bool autoApprove)
        {
            using var fixture = new TransferRequestRejectedEventHandlerTestsFixture()
                .WithEmployerParty()
                .AddCohortToMemoryDb()
                .AddTransferRequest(autoApprove);

            Assert.ThrowsAsync<DomainException>(() => fixture.Handle());
            Assert.That(fixture.Logger.HasErrors, Is.True);
        }
    }

    public class TransferRequestRejectedEventHandlerTestsFixture : IDisposable
    {
        private readonly Fixture _fixture;
        public FakeLogger<TransferRequestRejectedEvent> Logger { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public Cohort Cohort { get; set; }
        public DraftApprenticeship ExistingApprenticeshipDetails;
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public TransferRequestRejectedEvent TransferRequestRejectedEvent { get; set; }
        public TransferRequestRejectedEventHandler Handler { get; set; }
        public TransferRequest TransferRequest { get; }

        public TransferRequestRejectedEventHandlerTestsFixture()
        {
            _fixture = new Fixture();
            UnitOfWorkContext = new UnitOfWorkContext();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            TransferRequestRejectedEvent = _fixture.Create<TransferRequestRejectedEvent>();

            Logger = new FakeLogger<TransferRequestRejectedEvent>();
            Handler = new TransferRequestRejectedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), Logger);

            TransferRequest = new TransferRequest
            { Id = TransferRequestRejectedEvent.TransferRequestId, Status = TransferApprovalStatus.Pending, Cost = 1000, Cohort = Cohort };

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

        public TransferRequestRejectedEventHandlerTestsFixture AddTransferRequest(bool autoApproval)
        {
            TransferRequest.AutoApproval = autoApproval;

            Db.TransferRequests.Add(TransferRequest);
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
            Assert.That(Cohort.WithParty, Is.EqualTo(Party.Employer));
        }

        public void VerifyEntityIsBeingTracked()
        {
            var list = UnitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().Where(x => x.StateChangeType == UserAction.RejectTransferRequest).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(list, Has.Count.EqualTo(1));
                Assert.That(list[0].StateChangeType, Is.EqualTo(UserAction.RejectTransferRequest));
                Assert.That(list[0].EntityId, Is.EqualTo(Cohort.Id));
                Assert.That(list[0].UpdatingUserName, Is.EqualTo(TransferRequestRejectedEvent.UserInfo.UserDisplayName));
                Assert.That(list[0].UpdatingParty, Is.EqualTo(Party.TransferSender));
            });
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}