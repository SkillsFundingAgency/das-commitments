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
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using AutoFixture;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class RemoveAccountLegalEntityCommandHandlerTests
    {
        [Test]
        public async Task
            Handle_WhenAccountLegalEntityHasNotAlreadyBeenDeletedAndAccountProviderLegalEntitiesDoNotExist_ThenShouldNotPublishDeletedPermissionsEvent()
        {
            using var fixture = new RemoveAccountLegalEntityCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.UnitOfWorkContext.GetEvents().SingleOrDefault().Should().BeNull();
        }

        [Test]
        public async Task Handle_WhenAccountLegalEntityHasAlreadyBeenDeleted_ThenShouldThrowException()
        {
            using var fixture = new RemoveAccountLegalEntityCommandHandlerTestsFixture();
            fixture.SetAccountLegalEntityDeletedBeforeCommand();
            Func<Task> action = () => fixture.Handle();
            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task Handle_WhenAccountLegalEntityHasNotAlreadyBeenDeleted_ThenShouldDeleteAccountLegalEntity()
        {
            using var fixture = new RemoveAccountLegalEntityCommandHandlerTestsFixture();
            await fixture.Handle();
            fixture.AccountLegalEntity.Deleted.Should().Be(fixture.Command.Removed);
        }

        [Test]
        public async Task Handle_WhenCohortIsEmpty_ThenShouldMarkCohortAsDeletedAndEmitCohortDeletedEvent()
        {
            using var fixture = new RemoveAccountLegalEntityCommandHandlerTestsFixture();
            fixture.WithExistingCohort();
            await fixture.Handle();
            
            fixture.VerifyCohortIsMarkedAsDeletedAndEventIsEmitted();
        }

        [Test]
        public async Task Handle_WhenCohortIsNotEmpty_ThenShouldMarkApprenticeshipAsDeletedAndEmitApprenticeshipDeletedEvent()
        {
            using var fixture = new RemoveAccountLegalEntityCommandHandlerTestsFixture();
            fixture.WithExistingCohort().WithExistingDraftApprenticeship();
            await fixture.Handle();
            
            fixture.VerifyDraftApprenticeshipDeletedAndEventEmitted();
        }

        [Test]
        public async Task Handle_WhenCohortIsNotEmptyAndApprenticeshipIsApproved_ThenShouldThenShouldThrowDomainException()
        {
            using var fixture = new RemoveAccountLegalEntityCommandHandlerTestsFixture();
            fixture.WithExistingCohort()
                .WithExistingApprenticeship();
            
            Func<Task> action = () =>  fixture.Handle();
            await action.Should().ThrowAsync<DomainException>();
        }
    }

    public class RemoveAccountLegalEntityCommandHandlerTestsFixture : IDisposable
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
        public Apprenticeship Apprenticeship { get; private set; }

        private readonly Fixture _autoFixture;


        public RemoveAccountLegalEntityCommandHandlerTestsFixture()
        {
            _autoFixture = new Fixture();

            Now = DateTime.UtcNow;
            Account = ObjectActivator.CreateInstance<Account>().Set(a => a.Id, 1);
            AccountLegalEntity = ObjectActivator.CreateInstance<AccountLegalEntity>().Set(ale => ale.Id, 2)
                .Set(ale => ale.AccountId, Account.Id);
            Command = new RemoveAccountLegalEntityCommand(Account.Id, AccountLegalEntity.Id, Now.AddHours(-1));
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);

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

        public RemoveAccountLegalEntityCommandHandlerTestsFixture WithExistingDraftApprenticeship()
        {
            DraftApprenticeship = new DraftApprenticeship
            {
                FirstName = "Test",
                LastName = "Test",
                ReservationId = _autoFixture.Create<Guid>(),
                IsApproved = false
            };
            Cohort.Apprenticeships.Add(DraftApprenticeship);

            return this;
        }

        public RemoveAccountLegalEntityCommandHandlerTestsFixture WithExistingApprenticeship()
        {
            Apprenticeship = new Apprenticeship
            {
                FirstName = "Test",
                LastName = "Test",
                ReservationId = _autoFixture.Create<Guid>(),
                IsApproved = true
            };
            Cohort.Apprenticeships.Add(Apprenticeship);

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

            Assert.That(Cohort.IsDeleted, Is.True, "Cohort is not marked as deleted");
            Assert.That(emittedEvent.CohortId, Is.EqualTo(Cohort.Id));
            Assert.That(emittedEvent.AccountId, Is.EqualTo(Cohort.EmployerAccountId));
            Assert.That(emittedEvent.ProviderId, Is.EqualTo(Cohort.ProviderId));
        }

        public void VerifyDraftApprenticeshipDeletedAndEventEmitted()
        {
            var deleted = Cohort.DraftApprenticeships.SingleOrDefault();

            var emittedEvent =
                (DraftApprenticeshipDeletedEvent)UnitOfWorkContext.GetEvents()
                    .Single(x => x is DraftApprenticeshipDeletedEvent);

            Assert.That(deleted, Is.Null, "Draft apprenticeship record not deleted");

            emittedEvent.DraftApprenticeshipId = DraftApprenticeship.Id;
            emittedEvent.CohortId = Cohort.Id;
            emittedEvent.ReservationId = DraftApprenticeship.ReservationId;
            emittedEvent.Uln = DraftApprenticeship.Uln;
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}