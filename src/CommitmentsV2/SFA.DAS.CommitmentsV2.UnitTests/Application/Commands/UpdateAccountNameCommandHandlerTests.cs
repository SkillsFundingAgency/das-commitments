using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class UpdateAccountNameCommandHandlerTests : FluentTest<UpdateAccountNameCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenCommandIsHandledChronologically_ThenShouldUpdateAccountName()
        {
            return TestAsync(f => f.Handle(), f =>
            {
                f.Account.Name.Should().Be(f.Command.Name);
                f.Account.Updated.Should().Be(f.Command.Created);
            });
        }

        [Test]
        public Task Handle_WhenCommandIsHandledNonChronologically_ThenShouldNotUpdateAccountName()
        {
            return TestAsync(f => f.SetAccountUpdatedAfterCommand(), f => f.Handle(), f =>
            {
                f.Account.Name.Should().Be(f.OriginalAccountName);
                f.Account.Updated.Should().Be(f.Now);
            });
        }
    }

    public class UpdateAccountNameCommandHandlerTestsFixture
    {
        public Account Account { get; set; }
        public UpdateAccountNameCommand Command { get; set; }
        public IRequestHandler<UpdateAccountNameCommand, Unit> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public string OriginalAccountName { get; set; }
        public DateTime Now { get; set; }

        public UpdateAccountNameCommandHandlerTestsFixture()
        {
            OriginalAccountName = "Foo";
            Now = DateTime.UtcNow;
            Account = ObjectActivator.CreateInstance<Account>().Set(a => a.Id, 1).Set(a => a.Name, OriginalAccountName);

            Command = new UpdateAccountNameCommand(Account.Id, "Bar", Now.AddHours(-1));
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

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
    }
}
