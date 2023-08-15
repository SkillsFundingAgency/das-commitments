using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class CreateAccountCommandHandlerTests 
    {
        [Test]
        public async Task Handle_WhenHandlingCreateAccountCommand_ThenShouldCreateAccount()
        {
            using var fixture = new CreateAccountCommandHandlerTestFixture();
            await fixture.Handle();

            fixture.Db.Accounts.SingleOrDefault(a => a.Id == fixture.Command.AccountId)
                .Should().NotBeNull()
                .And.Match<Account>(a =>
                    a.Id == fixture.Command.AccountId &&
                    a.HashedId == fixture.Command.HashedId &&
                    a.PublicHashedId == fixture.Command.PublicHashedId &&
                    a.Name == fixture.Command.Name &&
                    a.Created == fixture.Command.Created);
        }
    }

    public class CreateAccountCommandHandlerTestFixture : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public CreateAccountCommand Command { get; set; }
        public IRequestHandler<CreateAccountCommand> Handler { get; set; }

        public CreateAccountCommandHandlerTestFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Command = new CreateAccountCommand(1, "AAA111", "AAA222", "Foo", DateTime.UtcNow);
            Handler = new CreateAccountCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), Mock.Of<ILogger<CreateAccountCommandHandler>>());
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}
