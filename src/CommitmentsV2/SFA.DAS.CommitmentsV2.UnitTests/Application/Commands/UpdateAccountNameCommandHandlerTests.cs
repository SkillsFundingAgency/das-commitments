using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class UpdateAccountNameCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenCommandIsHandledChronologically_ThenShouldUpdateAccountName()
        {
            using var fixture = new UpdateAccountNameCommandHandlerTestsFixture();
            await fixture.Handle();
            
            fixture.Account.Name.Should().Be(fixture.Command.Name);
            fixture.Account.Updated.Should().Be(fixture.Command.Created);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandledNonChronologically_ThenShouldNotUpdateAccountName()
        {
            using var fixture = new UpdateAccountNameCommandHandlerTestsFixture();
            fixture.SetAccountUpdatedAfterCommand();
            await fixture.Handle();
            
            fixture.Account.Name.Should().Be(fixture.OriginalAccountName);
            fixture.Account.Updated.Should().Be(fixture.Now);
        }
    }

    public class UpdateAccountNameCommandHandlerTestsFixture : IDisposable
    {
        public Account Account { get; set; }
        public UpdateAccountNameCommand Command { get; set; }
        public IRequestHandler<UpdateAccountNameCommand> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public string OriginalAccountName { get; set; }
        public DateTime Now { get; set; }

        public UpdateAccountNameCommandHandlerTestsFixture()
        {
            OriginalAccountName = "Foo";
            Now = DateTime.UtcNow;
            Account = ObjectActivator.CreateInstance<Account>().Set(a => a.Id, 1).Set(a => a.Name, OriginalAccountName);

            Command = new UpdateAccountNameCommand(Account.Id, "Bar", Now.AddHours(-1));
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);

            Db.Accounts.Add(Account);
            Db.SaveChanges();

            Handler = new UpdateAccountNameCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public UpdateAccountNameCommandHandlerTestsFixture SetAccountUpdatedAfterCommand()
        {
            Account.Set(a => a.Updated, Now);
            Db.SaveChanges();

            return this;
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}
