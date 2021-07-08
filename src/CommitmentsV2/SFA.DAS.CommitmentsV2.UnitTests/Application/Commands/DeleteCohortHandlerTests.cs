using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
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
            var f = new DeleteCohortHandlerTestsFixture();
            f.WithExistingCohort(Party.Employer).WithParty(Party.Employer);
            await f.Handle();
            f.VerifyCohortIsMarkedAsDeleted();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.TransferSender)]
        [TestCase(Party.None)]
        public void DeleteCohort_WhenCohortIsEmptyAndNotWithEmployer_ShouldThrowDomainException(Party party)
        {
            var f = new DeleteCohortHandlerTestsFixture();
            f.WithExistingCohort(Party.Employer).WithParty(party);

            Assert.ThrowsAsync<DomainException>(() => f.Handle());
        }

        [Test]
        public async Task DeleteCohort_WhenCohortIsNotEmptyAndWithEmployer_ShouldMarkRemoveTheApprentice()
        {
            var f = new DeleteCohortHandlerTestsFixture();
            f.WithExistingCohort(Party.Employer).WithParty(Party.Employer).WithExistingDraftApprenticeship();
            await f.Handle();
            f.VerifyDraftApprenticeshipDeleted();
        }

        [Test]
        public async Task DeleteCohort_WhenCohortIsNotEmptyAndWithEmployer_ShouldEmitEventForDraftApprenticeshipBeingDeleted()
        {
            var f = new DeleteCohortHandlerTestsFixture();
            f.WithExistingCohort(Party.Employer).WithParty(Party.Employer).WithExistingDraftApprenticeship();
            await f.Handle();
            f.VerifyEventEmittedWhenDraftApprenticeshipIsDeleted();
        }

        [Test]
        public async Task DeleteCohort_WhenNotApproved_ShouldEmitCohortDeletedEventWithNoPartyApprovals()
        {
            var f = new DeleteCohortHandlerTestsFixture();
            f.WithExistingCohort(Party.Employer).WithParty(Party.Employer);
            await f.Handle();
            f.VerifyCohortDeletedEventIsEmittedAndWasApprovedBy(Party.None);
        }

        [Test]
        public async Task DeleteCohort_WhenApprovedByProvider_ShouldEmitCohortDeletedEventWithProviderApproval()
        {
            var f = new DeleteCohortHandlerTestsFixture();
            f.WithExistingCohort(Party.Employer).WithParty(Party.Employer).WithExistingDraftApprenticeship()
                .WithProviderApproval();
            await f.Handle();
            f.VerifyCohortDeletedEventIsEmittedAndWasApprovedBy(Party.Provider);
        }

        [Test]
        public async Task DeleteCohort_WhenChangeOfPartyCohort_WhenWithProvider_ShouldEmitProviderRejectedChangeOfPartyRequestEvent()
        {
            var f = new DeleteCohortHandlerTestsFixture();
            f.WithExistingCohort(Party.Provider).WithParty(Party.Provider).WithExistingDraftApprenticeship().WithChangeOfParty(true);
               
            await f.Handle();
            f.VerifProviderRejectedChangeOfPartyRequestEvent();
        }

        [Test]
        public async Task DeleteCohort_WhenChangeOfPartyCohort_WhenWithEmployer_ShouldNotEmitProviderRejectedChangeOfPartyRequestEvent()
        {
            var f = new DeleteCohortHandlerTestsFixture();
            f.WithExistingCohort(Party.Employer).WithParty(Party.Employer).WithExistingDraftApprenticeship().WithChangeOfParty(true);

            await f.Handle();
            f.VerifProviderRejectedChangeOfPartyRequestEventIsNotPublished();
        }
    }

    public class DeleteCohortHandlerTestsFixture
    {
        public DeleteCohortCommand Command { get; private set; }
        public IRequestHandler<DeleteCohortCommand, Unit> Sut { get; }
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
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(warnings =>
                    warnings.Throw(RelationalEventId.QueryClientEvaluationWarning)).Options);

            Sut = new DeleteCohortHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), Logger.Object, AuthenticationService.Object);
            UnitOfWorkContext = new UnitOfWorkContext();

            DraftApprenticeship = new DraftApprenticeship
            {
                Id = _autoFixture.Create<long>(),
                FirstName = "Test",
                LastName = "Test",
                ReservationId = _autoFixture.Create<Guid>()
            };
        }

        public async Task Handle()
        {
            Db.SaveChanges();

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
            Assert.IsTrue(Cohort.IsDeleted, "Cohort is not marked as deleted");
        }

        public void VerifyDraftApprenticeshipDeleted()
        {
            var deleted = Cohort.DraftApprenticeships.SingleOrDefault();

            Assert.IsNull(deleted, "Draft apprenticeship record not deleted");
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

            Assert.AreEqual(Cohort.Id, emittedEvent.CohortId);
            Assert.AreEqual( Cohort.EmployerAccountId, emittedEvent.AccountId);
            Assert.AreEqual( Cohort.ProviderId,emittedEvent.ProviderId);
            Assert.IsTrue( emittedEvent.ApprovedBy.HasFlag(party));
        }

        public void VerifProviderRejectedChangeOfPartyRequestEvent()
        {
            var emittedEvent = (ProviderRejectedChangeOfPartyRequestEvent)UnitOfWorkContext.GetEvents().Single(x => x is ProviderRejectedChangeOfPartyRequestEvent);

            Assert.AreEqual(Cohort.EmployerAccountId, emittedEvent.EmployerAccountId);
            Assert.AreEqual(Cohort.Provider.Name, emittedEvent.TrainingProviderName);
            Assert.AreEqual(Cohort.ChangeOfPartyRequestId, emittedEvent.ChangeOfPartyRequestId);
            Assert.AreEqual(Cohort.LastUpdatedByEmployerEmail, emittedEvent.RecipientEmailAddress);
            Assert.AreEqual(Cohort.AccountLegalEntity.Name, emittedEvent.EmployerName);
            Assert.AreEqual($"Test Test", emittedEvent.ApprenticeName);
        }

        public void VerifProviderRejectedChangeOfPartyRequestEventIsNotPublished()
        {
            Assert.IsNull(UnitOfWorkContext.GetEvents().FirstOrDefault(x => x is ProviderRejectedChangeOfPartyRequestEvent));
        }

        internal void WithChangeOfParty(bool v)
        {
            if (v)
                Cohort.ChangeOfPartyRequestId = 1;
        }
    }
}
