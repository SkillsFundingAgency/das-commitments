using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class UpdateAccountLegalEntityNameCommandHandlerTest
    {
        [Test]
        public async Task Handle_WhenCommandIsHandledChronologically_ThenShouldUpdateAccountLegalEntityName()
        {
            using var fixture = new UpdateAccountLegalEntityNameCommandHandlerTestsFixture();
            await fixture.Handle();
            
            fixture.AccountLegalEntity.Name.Should().Be(fixture.Command.Name);
            fixture.AccountLegalEntity.Updated.Should().Be(fixture.Command.Created);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandledNonChronologically_ThenShouldNotUpdateAccountLegalEntityName()
        {
            using var fixture = new UpdateAccountLegalEntityNameCommandHandlerTestsFixture();
            fixture.SetAccountLegalEntityUpdatedAfterCommand();
            await fixture.Handle();
            
            fixture.AccountLegalEntity.Name.Should().Be(fixture.OriginalAccountLegalEntityName);
            fixture.AccountLegalEntity.Updated.Should().Be(fixture.Now);
        }
    }

    public class UpdateAccountLegalEntityNameCommandHandlerTestsFixture : IDisposable
    {
        public AccountLegalEntity AccountLegalEntity { get; set; }
        public UpdateAccountLegalEntityNameCommand Command { get; set; }
        public IRequestHandler<UpdateAccountLegalEntityNameCommand> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public string OriginalAccountLegalEntityName { get; set; }
        public DateTime Now { get; set; }

        public UpdateAccountLegalEntityNameCommandHandlerTestsFixture()
        {
            OriginalAccountLegalEntityName = "Foo";
            Now = DateTime.UtcNow;
            AccountLegalEntity = ObjectActivator.CreateInstance<AccountLegalEntity>().Set(ale => ale.Id, 1).Set(ale => ale.Name, OriginalAccountLegalEntityName);
            Command = new UpdateAccountLegalEntityNameCommand(AccountLegalEntity.Id, "Bar", Now.AddHours(-1));
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);

            Db.AccountLegalEntities.Add(AccountLegalEntity);
            Db.SaveChanges();

            Handler = new UpdateAccountLegalEntityNameCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public UpdateAccountLegalEntityNameCommandHandlerTestsFixture SetAccountLegalEntityUpdatedAfterCommand()
        {
            AccountLegalEntity.Set(a => a.Updated, Now);
            Db.SaveChanges();

            return this;
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}