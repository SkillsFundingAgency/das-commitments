using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class RemoveAccountLegalEntityCommandHandlerTests : FluentTest<RemoveAccountLegalEntityCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenAccountLegalEntityHasNotAlreadyBeenDeleted_ThenShouldDeleteAccountLegalEntity()
        {
            return TestAsync(f => f.Handle(), f => f.AccountLegalEntity.Deleted.Should().Be(f.Command.Removed));
        }

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
    }

    public class RemoveAccountLegalEntityCommandHandlerTestsFixture
    {
        public Account Account { get; set; }
        public AccountLegalEntity AccountLegalEntity { get; set; }
        public RemoveAccountLegalEntityCommand Command { get; set; }
        public IRequestHandler<RemoveAccountLegalEntityCommand, Unit> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IUnitOfWorkContext UnitOfWorkContext { get; set; }
        public DateTime Now { get; set; }

        public RemoveAccountLegalEntityCommandHandlerTestsFixture()
        {
            Now = DateTime.UtcNow;
            Account = ObjectActivator.CreateInstance<Account>().Set(a => a.Id, 1);
            AccountLegalEntity = ObjectActivator.CreateInstance<AccountLegalEntity>().Set(ale => ale.Id, 2).Set(ale => ale.AccountId, Account.Id);
            Command = new RemoveAccountLegalEntityCommand(Account.Id, AccountLegalEntity.Id, Now.AddHours(-1));
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            Db.Accounts.Add(Account);
            Db.AccountLegalEntities.Add(AccountLegalEntity);
            Db.SaveChanges();

            Handler = new RemoveAccountLegalEntityCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
            UnitOfWorkContext = new UnitOfWorkContext();
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public RemoveAccountLegalEntityCommandHandlerTestsFixture SetAccountLegalEntityDeletedBeforeCommand()
        {
            AccountLegalEntity.Set(ale => ale.Deleted, Command.Removed.AddHours(-1));
            Db.SaveChanges();

            return this;
        }
    }
}
