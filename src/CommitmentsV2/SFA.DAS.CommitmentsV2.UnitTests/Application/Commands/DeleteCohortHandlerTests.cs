using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteCohort;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class DeleteCohortHandlerTests 
    {

        [Test]
        public async Task DeleteCohort_WhenCohortIsEmptyAndWithEmployer_ShouldMarkCohortAsDeleted()
        {
            using var fixture = new DeleteCohortHandlerTestsFixture();
            fixture.WithExistingCohort(Party.Employer).WithParty(Party.Employer);
            await fixture.Handle();
            fixture.VerifyCohortIsMarkedAsDeleted();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.TransferSender)]
        [TestCase(Party.None)]
        public void DeleteCohort_WhenCohortIsEmptyAndNotWithEmployer_ShouldThrowDomainException(Party party)
        {
            using var fixture = new DeleteCohortHandlerTestsFixture();
            fixture.WithExistingCohort(Party.Employer).WithParty(party);

            Assert.ThrowsAsync<DomainException>(() => fixture.Handle());
        }

        [Test]
        public async Task DeleteCohort_WhenCohortIsNotEmptyAndWithEmployer_ShouldMarkRemoveTheApprentice()
        {
            using var fixture = new DeleteCohortHandlerTestsFixture();
            fixture.WithExistingCohort(Party.Employer).WithParty(Party.Employer).WithExistingDraftApprenticeship();
            await fixture.Handle();
            fixture.VerifyDraftApprenticeshipDeleted();
        }

        [Test]
        public async Task DeleteCohort_WhenCohortIsNotEmptyAndWithEmployer_ShouldEmitEventForDraftApprenticeshipBeingDeleted()
        {
            using var fixture = new DeleteCohortHandlerTestsFixture();
            fixture.WithExistingCohort(Party.Employer).WithParty(Party.Employer).WithExistingDraftApprenticeship();
            await fixture.Handle();
            fixture.VerifyEventEmittedWhenDraftApprenticeshipIsDeleted();
        }

        [Test]
        public async Task DeleteCohort_WhenNotApproved_ShouldEmitCohortDeletedEventWithNoPartyApprovals()
        {
            using var fixture = new DeleteCohortHandlerTestsFixture();
            fixture.WithExistingCohort(Party.Employer).WithParty(Party.Employer);
            await fixture.Handle();
            fixture.VerifyCohortDeletedEventIsEmittedAndWasApprovedBy(Party.None);
        }

        [Test]
        public async Task DeleteCohort_WhenApprovedByProvider_ShouldEmitCohortDeletedEventWithProviderApproval()
        {
            using var fixture = new DeleteCohortHandlerTestsFixture();
            fixture.WithExistingCohort(Party.Employer).WithParty(Party.Employer).WithExistingDraftApprenticeship()
                .WithProviderApproval();
            await fixture.Handle();
            fixture.VerifyCohortDeletedEventIsEmittedAndWasApprovedBy(Party.Provider);
        }

        [Test]
        public async Task DeleteCohort_WhenChangeOfPartyCohort_WhenWithProvider_ShouldEmitProviderRejectedChangeOfPartyRequestEvent()
        {
            using var fixture = new DeleteCohortHandlerTestsFixture();
            fixture.WithExistingCohort(Party.Provider).WithParty(Party.Provider).WithExistingDraftApprenticeship().WithChangeOfParty(true);
               
            await fixture.Handle();
            fixture.VerifProviderRejectedChangeOfPartyRequestEvent();
        }

        [Test]
        public async Task DeleteCohort_WhenChangeOfPartyCohort_WhenWithEmployer_ShouldNotEmitProviderRejectedChangeOfPartyRequestEvent()
        {
            using var fixture = new DeleteCohortHandlerTestsFixture();
            fixture.WithExistingCohort(Party.Employer).WithParty(Party.Employer).WithExistingDraftApprenticeship().WithChangeOfParty(true);

            await fixture.Handle();
            fixture.VerifProviderRejectedChangeOfPartyRequestEventIsNotPublished();
        }
    }

    public class DeleteCohortHandlerTestsFixture : IDisposable
    {
        public DeleteCohortCommand Command { get; private set; }
        public IRequestHandler<DeleteCohortCommand> Sut { get; }
        public ProviderCommitmentsDbContext Db { get; }
        public Mock<ILogger<DeleteCohortHandler>> Logger { get; }
        public Mock<IAuthenticationService> AuthenticationService { get; }
        public IUnitOfWorkContext UnitOfWorkContext { get; private set; }
        public long CohortId { get; private set; }
        public UserInfo UserInfo { get; private set; }
        public Party Party { get; private set; }
        public Cohort Cohort { get; private set; }
        public DraftApprenticeship DraftApprenticeship { get; private set; }

        private Fixture _autoFixture;

        public DeleteCohortHandlerTestsFixture()
        {
            _autoFixture = new Fixture();

            CohortId = _autoFixture.Create<long>();
            UserInfo = _autoFixture.Create<UserInfo>();
            Logger = new Mock<ILogger<DeleteCohortHandler>>();
            AuthenticationService = new Mock<IAuthenticationService>();

            Command = new DeleteCohortCommand { CohortId = CohortId, UserInfo = UserInfo} ;
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);

            Sut = new DeleteCohortHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), Logger.Object, AuthenticationService.Object);
            UnitOfWorkContext = new UnitOfWorkContext();

            DraftApprenticeship = new DraftApprenticeship
            {
                FirstName = "Test",
                LastName = "Test",
                ReservationId = _autoFixture.Create<Guid>()
            };
        }

        public async Task Handle()
        {
            await Db.SaveChangesAsync();

            await Sut.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public DeleteCohortHandlerTestsFixture WithParty(Party party)
        {
            Party = party;
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(Party);
            return this;
        }

        public DeleteCohortHandlerTestsFixture WithExistingDraftApprenticeship()
        {
            Cohort.Apprenticeships.Add(DraftApprenticeship);
            return this;
        }

        public DeleteCohortHandlerTestsFixture WithProviderApproval()
        {
            Cohort.Approvals = Party.Provider;
            return this;
        }

        public DeleteCohortHandlerTestsFixture WithExistingCohort(Party creatingParty)
        {
            Cohort = new Cohort
            {
                Id = CohortId,
                EditStatus = creatingParty.ToEditStatus(),
                WithParty = creatingParty,
                ProviderId = _autoFixture.Create<long>(),
                EmployerAccountId = _autoFixture.Create<long>(),
                Provider = new Provider
                {
                    Name = "ProviderName"
                },
                AccountLegalEntity = new AccountLegalEntity(new Account(), 1, 1, "1", "XX", "EmployerName", OrganisationType.Other, "XX", DateTime.Now),
                LastUpdatedByEmployerEmail = "abc@abc.com"
            };

            Db.Cohorts.Add(Cohort);

            return this;
        }

        public void VerifyCohortIsMarkedAsDeleted()
        {
            Assert.That(Cohort.IsDeleted, Is.True, "Cohort is not marked as deleted");
        }

        public void VerifyDraftApprenticeshipDeleted()
        {
            var deleted = Cohort.DraftApprenticeships.SingleOrDefault();

            Assert.That(deleted, Is.Null, "Draft apprenticeship record not deleted");
        }

        public void VerifyEventEmittedWhenDraftApprenticeshipIsDeleted()
        {
            var emittedEvent = (DraftApprenticeshipDeletedEvent)UnitOfWorkContext.GetEvents().Single(x => x is DraftApprenticeshipDeletedEvent);

            emittedEvent.DraftApprenticeshipId = DraftApprenticeship.Id;
            emittedEvent.CohortId = Cohort.Id;
            emittedEvent.ReservationId = DraftApprenticeship.ReservationId;
            emittedEvent.Uln = DraftApprenticeship.Uln;
        }

        public void VerifyCohortDeletedEventIsEmittedAndWasApprovedBy(Party party)
        {
            var emittedEvent = (CohortDeletedEvent)UnitOfWorkContext.GetEvents().Single(x => x is CohortDeletedEvent);

            Assert.Multiple(() =>
            {
                Assert.That(emittedEvent.CohortId, Is.EqualTo(Cohort.Id));
                Assert.That(emittedEvent.AccountId, Is.EqualTo(Cohort.EmployerAccountId));
                Assert.That(emittedEvent.ProviderId, Is.EqualTo(Cohort.ProviderId));
                Assert.That(emittedEvent.ApprovedBy.HasFlag(party), Is.True);
            });
        }

        public void VerifProviderRejectedChangeOfPartyRequestEvent()
        {
            var emittedEvent = (ProviderRejectedChangeOfPartyRequestEvent)UnitOfWorkContext.GetEvents().Single(x => x is ProviderRejectedChangeOfPartyRequestEvent);

            Assert.Multiple(() =>
            {
                Assert.That(emittedEvent.EmployerAccountId, Is.EqualTo(Cohort.EmployerAccountId));
                Assert.That(emittedEvent.TrainingProviderName, Is.EqualTo(Cohort.Provider.Name));
                Assert.That(emittedEvent.ChangeOfPartyRequestId, Is.EqualTo(Cohort.ChangeOfPartyRequestId));
                Assert.That(emittedEvent.RecipientEmailAddress, Is.EqualTo(Cohort.LastUpdatedByEmployerEmail));
                Assert.That(emittedEvent.EmployerName, Is.EqualTo(Cohort.AccountLegalEntity.Name));
                Assert.That(emittedEvent.ApprenticeName, Is.EqualTo($"Test Test"));
            });
        }

        public void VerifProviderRejectedChangeOfPartyRequestEventIsNotPublished()
        {
            Assert.That(UnitOfWorkContext.GetEvents().FirstOrDefault(x => x is ProviderRejectedChangeOfPartyRequestEvent), Is.Null);
        }

        internal void WithChangeOfParty(bool value)
        {
            if (value)
                Cohort.ChangeOfPartyRequestId = 1;
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
