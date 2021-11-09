using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class CreateAccountCommandHandlerTests : FluentTest<CreateAccountCommandHandlerTestFixture>
    {
        [Test]
        public Task Handle_WhenHandlingCreateAccountCommand_ThenShouldCreateAccount()
        {
            return TestAsync(f => f.Handle(), f => f.Db.Accounts.SingleOrDefault(a => a.Id == f.Command.AccountId).Should().NotBeNull()
                .And.Match<Account>(a =>
                    a.Id == f.Command.AccountId &&
                    a.HashedId == f.Command.HashedId &&
                    a.PublicHashedId == f.Command.PublicHashedId &&
                    a.Name == f.Command.Name &&
                    a.Created == f.Command.Created));
        }
    }

    public class CreateAccountCommandHandlerTestFixture
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public CreateAccountCommand Command { get; set; }
        public IRequestHandler<CreateAccountCommand, Unit> Handler { get; set; }

        public CreateAccountCommandHandlerTestFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Command = new CreateAccountCommand(1, "AAA111", "AAA222", "Foo", DateTime.UtcNow);
            Handler = new CreateAccountCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }
    }
}
