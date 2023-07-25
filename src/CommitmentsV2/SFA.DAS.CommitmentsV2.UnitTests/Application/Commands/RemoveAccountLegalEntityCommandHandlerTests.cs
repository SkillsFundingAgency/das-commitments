using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using AutoFixture;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class RemoveAccountLegalEntityCommandHandlerTests : FluentTest<RemoveAccountLegalEntityCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenAccountLegalEntityHasNotAlreadyBeenDeletedAndAccountProviderLegalEntitiesDoNotExist_ThenShouldNotPublishDeletedPermissionsEvent()
        {
            return TestAsync(f => f.Handle(), f => f.UnitOfWorkContext.GetEvents().SingleOrDefault().Should().BeNull());
        }

        [Test]
        public Task Handle_WhenAccountLegalEntityHasAlreadyBeenDeleted_ThenShouldThrowException()
        {
            return TestExceptionAsync(f => f.SetAccountLegalEntityDeletedBeforeCommand(),
                f => f.Handle(),
                (f, r) => r.Should().ThrowAsync<InvalidOperationException>());
        }

        [Test]
        public Task Handle_WhenAccountLegalEntityHasNotAlreadyBeenDeleted_ThenShouldDeleteAccountLegalEntity()
        {
            return TestAsync(f => f.Handle(), f => f.AccountLegalEntity.Deleted.Should().Be(f.Command.Removed));
        }

        [Test]
        public Task Handle_WhenCohortIsEmpty_ThenShouldMarkCohortAsDeletedAndEmitCohortDeletedEvent()
        {
            return TestAsync(f => f.WithExistingCohort(),
                 f => f.Handle(),
                 f => f.VerifyCohortIsMarkedAsDeletedAndEventIsEmitted());
        }

        [Test]
        public Task Handle_WhenCohortIsNotEmpty_ThenShouldMarkApprenticeshipAsDeletedAndEmitApprenticeshipDeletedEvent()
        {
            return TestAsync(f => f.WithExistingCohort().WithExistingDraftApprenticeship(false),
                f => f.Handle(),
                f => f.VerifyDraftApprenticeshipDeletedAndEventEmitted());
        }

        [Test]
        public Task Handle_WhenCohortIsNotEmptyAndApprenticeshipIsApproved_ThenShouldThenShouldThrowDomainException()
        {
            return TestExceptionAsync(f => f.WithExistingCohort().WithExistingDraftApprenticeship(true),
                f => f.Handle(),
                (f, r) => r.Should().ThrowAsync<DomainException>());
        }

    }

    public class RemoveAccountLegalEntityCommandHandlerTestsFixture
    {
        public Account Account { get; set; }
        public AccountLegalEntity AccountLegalEntity { get; set; }
        public RemoveAccountLegalEntityCommand Command { get; set; }
        public IRequestHandler<RemoveAccountLegalEntityCommand> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IUnitOfWorkContext UnitOfWorkContext { get; set; }
        public DateTime Now { get; set; }
        public Cohort Cohort { get; private set; }
        public DraftApprenticeship DraftApprenticeship { get; private set; }

        private readonly Fixture _autoFixture;


        public RemoveAccountLegalEntityCommandHandlerTestsFixture()
        {

            _autoFixture = new Fixture();

            Now = DateTime.UtcNow;
            Account = ObjectActivator.CreateInstance<Account>().Set(a => a.Id, 1);
            AccountLegalEntity = ObjectActivator.CreateInstance<AccountLegalEntity>().Set(ale => ale.Id, 2).Set(ale => ale.AccountId, Account.Id);
            Command = new RemoveAccountLegalEntityCommand(Account.Id, AccountLegalEntity.Id, Now.AddHours(-1));
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            Db.Accounts.Add(Account);
            Db.AccountLegalEntities.Add(AccountLegalEntity);

            Handler = new RemoveAccountLegalEntityCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
            UnitOfWorkContext = new UnitOfWorkContext();

        }

        public async Task Handle()
        {
            await Db.SaveChangesAsync();

            await Handler.Handle(Command, CancellationToken.None);

        }

        public RemoveAccountLegalEntityCommandHandlerTestsFixture SetAccountLegalEntityDeletedBeforeCommand()
        {
            AccountLegalEntity.Set(ale => ale.Deleted, Command.Removed.AddHours(-1));

            return this;
        }

        public RemoveAccountLegalEntityCommandHandlerTestsFixture WithExistingDraftApprenticeship(bool isApproved)
        {
            DraftApprenticeship = new DraftApprenticeship
            {
                FirstName = "Test",
                LastName = "Test",
                ReservationId = _autoFixture.Create<Guid>(),
                IsApproved = isApproved
            };

            Cohort.Apprenticeships.Add(DraftApprenticeship);

            return this;
        }

        public RemoveAccountLegalEntityCommandHandlerTestsFixture WithExistingCohort()
        {
            Cohort = new Cohort
            {
                Id = _autoFixture.Create<long>(),
                ProviderId = _autoFixture.Create<long>(),
                EmployerAccountId = _autoFixture.Create<long>(),
                AccountLegalEntity = AccountLegalEntity
            };

            Db.Cohorts.Add(Cohort);

            return this;
        }

        public void VerifyCohortIsMarkedAsDeletedAndEventIsEmitted()
        {

            var emittedEvent = (CohortDeletedEvent)UnitOfWorkContext.GetEvents().Single(x => x is CohortDeletedEvent);

            Assert.IsTrue(Cohort.IsDeleted, "Cohort is not marked as deleted");
            Assert.AreEqual(Cohort.Id, emittedEvent.CohortId);
            Assert.AreEqual(Cohort.EmployerAccountId, emittedEvent.AccountId);
            Assert.AreEqual(Cohort.ProviderId, emittedEvent.ProviderId);
        }

        public void VerifyDraftApprenticeshipDeletedAndEventEmitted()
        {
            var deleted = Cohort.DraftApprenticeships.SingleOrDefault();

            var emittedEvent = (DraftApprenticeshipDeletedEvent)UnitOfWorkContext.GetEvents().Single(x => x is DraftApprenticeshipDeletedEvent);

            Assert.IsNull(deleted, "Draft apprenticeship record not deleted");

            emittedEvent.DraftApprenticeshipId = DraftApprenticeship.Id;
            emittedEvent.CohortId = Cohort.Id;
            emittedEvent.ReservationId = DraftApprenticeship.ReservationId;
            emittedEvent.Uln = DraftApprenticeship.Uln;
        }
    }
}
