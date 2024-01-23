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
using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddAccountLegalEntityCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingAddAccountLegalEntityCommand_ThenShouldAddAccountLegalEntity2()
        {
            using var fixture = new AddAccountLegalEntityCommandHandlerTestsFixture();

            await fixture.Handle();

            var result =fixture.Db.AccountLegalEntities.SingleOrDefault(ale => ale.Id == fixture.Command.AccountLegalEntityId);
                
            result
                .Should()
                .NotBeNull()
                .And.Match<AccountLegalEntity>(a =>
                    a.Id == fixture.Command.AccountLegalEntityId &&
                    a.PublicHashedId == fixture.Command.AccountLegalEntityPublicHashedId &&
                    a.Account == fixture.Account &&
                    a.AccountId == fixture.Command.AccountId &&
                    a.MaLegalEntityId == fixture.Command.MaLegalEntityId &&
                    a.Name == fixture.Command.OrganisationName &&
                    a.OrganisationType == fixture.Command.OrganisationType &&
                    a.LegalEntityId == fixture.Command.OrganisationReferenceNumber &&
                    a.Address == fixture.Command.OrganisationAddress &&
                    a.Created == fixture.Command.Created);
        }
    }

    public class AddAccountLegalEntityCommandHandlerTestsFixture : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public AddAccountLegalEntityCommand Command { get; set; }
        public IRequestHandler<AddAccountLegalEntityCommand> Handler { get; set; }
        public Account Account { get; set; }

        public AddAccountLegalEntityCommandHandlerTestsFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);

            Account = ObjectActivator.CreateInstance<Account>().Set(a => a.Id, 1);

            Db.Accounts.Add(Account);
            Db.SaveChanges();

            Command = new AddAccountLegalEntityCommand(Account.Id, 2, 202, "ALE123", "Foo",
                OrganisationType.CompaniesHouse, "REFNo", "Address", DateTime.UtcNow);

            Handler = new AddAccountLegalEntityCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), Mock.Of<ILogger<AddAccountLegalEntityCommandHandler>>());
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