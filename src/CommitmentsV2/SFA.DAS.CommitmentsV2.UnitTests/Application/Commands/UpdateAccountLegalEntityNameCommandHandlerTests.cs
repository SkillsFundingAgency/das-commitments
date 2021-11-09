using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class UpdateAccountLegalEntityNameCommandHandlerTests : FluentTest<UpdateAccountLegalEntityNameCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenCommandIsHandledChronologically_ThenShouldUpdateAccountLegalEntityName()
        {
            return TestAsync(f => f.Handle(), f =>
            {
                f.AccountLegalEntity.Name.Should().Be(f.Command.Name);
                f.AccountLegalEntity.Updated.Should().Be(f.Command.Created);
            });
        }

        [Test]
        public Task Handle_WhenCommandIsHandledNonChronologically_ThenShouldNotUpdateAccountLegalEntityName()
        {
            return TestAsync(f => f.SetAccountLegalEntityUpdatedAfterCommand(), f => f.Handle(), f =>
            {
                f.AccountLegalEntity.Name.Should().Be(f.OriginalAccountLegalEntityName);
                f.AccountLegalEntity.Updated.Should().Be(f.Now);
            });
        }
    }

    public class UpdateAccountLegalEntityNameCommandHandlerTestsFixture
    {
        public AccountLegalEntity AccountLegalEntity { get; set; }
        public UpdateAccountLegalEntityNameCommand Command { get; set; }
        public IRequestHandler<UpdateAccountLegalEntityNameCommand, Unit> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public string OriginalAccountLegalEntityName { get; set; }
        public DateTime Now { get; set; }

        public UpdateAccountLegalEntityNameCommandHandlerTestsFixture()
        {
            OriginalAccountLegalEntityName = "Foo";
            Now = DateTime.UtcNow;
            AccountLegalEntity = ObjectActivator.CreateInstance<AccountLegalEntity>().Set(ale => ale.Id, 1).Set(ale => ale.Name, OriginalAccountLegalEntityName);
            Command = new UpdateAccountLegalEntityNameCommand(AccountLegalEntity.Id, "Bar", Now.AddHours(-1));
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

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
    }
}