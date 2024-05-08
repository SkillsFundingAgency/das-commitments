using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class UpdateLevyStatusToLevyCommandHandlerTests
    {
        [Test]
        public void Handle_WhenHandlingCommand_ThenShouldUpdateTheLevyStatus()
        {
            var fixture = new UpdateLevyStatusToLevyCommandHandlerTestsFixture();
            fixture.SetAccount()
                .Handle();

            Assert.That(fixture.IsValid(ApprenticeshipEmployerType.Levy), Is.True);
        }

        [Test]
        public void Handle_WhenHandlingCommand_AndAccountNotFound_ThenShouldnotUpdateTheLevyStatus()
        {
            var fixture = new UpdateLevyStatusToLevyCommandHandlerTestsFixture();
            fixture.SetAccount();
            fixture.Command.AccountId = 2;
            fixture.Handle();

            Assert.That(fixture.IsValid(ApprenticeshipEmployerType.NonLevy), Is.True);
        }
    }

    public class UpdateLevyStatusToLevyCommandHandlerTestsFixture
    {
        public IFixture AutoFixture { get; set; }
        public UpdateLevyStatusToLevyCommand Command { get; set; }
        public Mock<ProviderCommitmentsDbContext> Db { get; set; }
        public IRequestHandler<UpdateLevyStatusToLevyCommand> Handler { get; set; }
        public long AccountId { get; set; }

        public UpdateLevyStatusToLevyCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            AccountId = 1;
            Command = new UpdateLevyStatusToLevyCommand { AccountId = AccountId };
            Db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };
            Handler = new UpdateLevyStatusToLevyCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db.Object), Mock.Of<ILogger<UpdateLevyStatusToLevyCommandHandler>>());

            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken.None);
        }

        public UpdateLevyStatusToLevyCommandHandlerTestsFixture SetAccount()
        {
            var account = new Account(AccountId, "", "", "", DateTime.UtcNow);

            Db.Object.Accounts.Add(account);
            Db.Object.SaveChanges();

            return this;
        }

        public bool IsValid(ApprenticeshipEmployerType levyStatus)
        {
            return levyStatus == Db.Object.Accounts.First().LevyStatus;
        }
    }
}