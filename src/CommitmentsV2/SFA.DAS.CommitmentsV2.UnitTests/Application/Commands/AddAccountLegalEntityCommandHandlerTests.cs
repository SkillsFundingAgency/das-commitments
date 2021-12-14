using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddAccountLegalEntityCommandHandlerTests : FluentTest<AddAccountLegalEntityCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingAddAccountLegalEntityCommand_ThenShouldAddAccountLegalEntity()
        {
            return TestAsync(f => f.Handle(), f => f.Db.AccountLegalEntities.SingleOrDefault(ale => ale.Id == f.Command.AccountLegalEntityId).Should().NotBeNull()
                .And.Match<AccountLegalEntity>(a =>
                    a.Id == f.Command.AccountLegalEntityId &&
                    a.PublicHashedId == f.Command.AccountLegalEntityPublicHashedId &&
                    a.Account == f.Account &&
                    a.AccountId == f.Command.AccountId &&
                    a.MaLegalEntityId == f.Command.MaLegalEntityId &&
                    a.Name == f.Command.OrganisationName &&
                    a.OrganisationType == f.Command.OrganisationType &&
                    a.LegalEntityId == f.Command.OrganisationReferenceNumber &&
                    a.Address == f.Command.OrganisationAddress &&
                    a.Created == f.Command.Created));
        }
    }

    public class AddAccountLegalEntityCommandHandlerTestsFixture
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public AddAccountLegalEntityCommand Command { get; set; }
        public IRequestHandler<AddAccountLegalEntityCommand, Unit> Handler { get; set; }
        public Account Account { get; set; }

        public AddAccountLegalEntityCommandHandlerTestsFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            Account = ObjectActivator.CreateInstance<Account>().Set(a => a.Id, 1);

            Db.Accounts.Add(Account);
            Db.SaveChanges();

            Command = new AddAccountLegalEntityCommand(Account.Id, 2,  202, "ALE123", "Foo",
                OrganisationType.CompaniesHouse, "REFNo", "Address", DateTime.UtcNow);

            Handler = new AddAccountLegalEntityCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }
    }
}