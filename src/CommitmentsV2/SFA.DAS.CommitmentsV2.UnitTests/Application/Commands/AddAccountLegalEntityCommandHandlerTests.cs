using Microsoft.Extensions.Logging;
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

        [Test]
        public async Task Handle_WhenAccountLegalEntityAlreadyExists_ThenShouldLeaveTheExistingRow()
        {
            using var fixture = new AddAccountLegalEntityCommandHandlerTestsFixture();
            fixture.WithExistingAccountLegalEntity();

            await fixture.Handle();

            var result = fixture.Db.AccountLegalEntities.IgnoreQueryFilters().Single();
            result.Name.Should().Be("Existing");
            result.MaLegalEntityId.Should().Be(101);
            result.Deleted.Should().BeNull();
        }

        [Test]
        public async Task Handle_WhenAccountLegalEntityAlreadyExistsAndIsDeleted_ThenShouldNotClearDeleted()
        {
            using var fixture = new AddAccountLegalEntityCommandHandlerTestsFixture();
            var deletedOn = new DateTime(2026, 9, 21, 8, 56, 0, DateTimeKind.Utc);
            fixture.WithExistingAccountLegalEntity(deletedOn);

            await fixture.Handle();

            var result = fixture.Db.AccountLegalEntities.IgnoreQueryFilters().Single();
            result.Name.Should().Be("Existing");
            result.Deleted.Should().Be(deletedOn);
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

        public void WithExistingAccountLegalEntity(DateTime? deletedOn = null)
        {
            var entity = new AccountLegalEntity(
                Account,
                Command.AccountLegalEntityId,
                101,
                "EXISTINGREF",
                "EXST01",
                "Existing",
                OrganisationType.CompaniesHouse,
                "Existing address",
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            if (deletedOn.HasValue)
            {
                entity.Delete(deletedOn.Value);
            }

            Db.AccountLegalEntities.Add(entity);
            Db.SaveChanges();
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}