using FluentAssertions;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLevyStatus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class UpdateAccountLevyStatusCommandHandlerTests
{
    [Test]
    public void Handle_WhenHandlingCommand_ThenShouldUpdateLevyStatusToLevy()
    {
        var fixture = new UpdateAccountLevyStatusCommandHandlerTestsFixture();
        fixture.SetAccount()
            .SetLevyStatus(ApprenticeshipEmployerType.Levy)
            .Handle();

        fixture.AccountLevyStatus.Should().Be(ApprenticeshipEmployerType.Levy);
    }

    [Test]
    public void Handle_WhenHandlingCommand_ThenShouldUpdateLevyStatusToNonLevy()
    {
        var fixture = new UpdateAccountLevyStatusCommandHandlerTestsFixture();
        fixture.SetAccount()
            .SetLevyStatus(ApprenticeshipEmployerType.NonLevy)
            .Handle();

        fixture.AccountLevyStatus.Should().Be(ApprenticeshipEmployerType.NonLevy);
    }

    [Test]
    public void Handle_WhenAccountNotFound_ThenShouldNotUpdateLevyStatus()
    {
        var fixture = new UpdateAccountLevyStatusCommandHandlerTestsFixture();
        fixture.SetAccount();
        fixture.Command.AccountId = 2;
        fixture.SetLevyStatus(ApprenticeshipEmployerType.Levy)
            .Handle();

        fixture.AccountLevyStatus.Should().Be(ApprenticeshipEmployerType.NonLevy);
    }
}

public class UpdateAccountLevyStatusCommandHandlerTestsFixture
{
    public UpdateAccountLevyStatusCommand Command { get; set; }
    public Mock<ProviderCommitmentsDbContext> Db { get; set; }
    public IRequestHandler<UpdateAccountLevyStatusCommand> Handler { get; set; }
    public long AccountId { get; set; }

    public UpdateAccountLevyStatusCommandHandlerTestsFixture()
    {
        AccountId = 1;
        Command = new UpdateAccountLevyStatusCommand { AccountId = AccountId };
        Db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options) { CallBase = true };
        Handler = new UpdateAccountLevyStatusCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db.Object), Mock.Of<ILogger<UpdateAccountLevyStatusCommandHandler>>());
    }

    public UpdateAccountLevyStatusCommandHandlerTestsFixture SetAccount()
    {
        var account = new Account(AccountId, "", "", "", DateTime.UtcNow);

        Db.Object.Accounts.Add(account);
        Db.Object.SaveChanges();

        return this;
    }

    public UpdateAccountLevyStatusCommandHandlerTestsFixture SetLevyStatus(ApprenticeshipEmployerType levyStatus)
    {
        Command.LevyStatus = levyStatus;
        return this;
    }

    public void Handle()
    {
        Handler.Handle(Command, CancellationToken.None).GetAwaiter().GetResult();
    }

    public ApprenticeshipEmployerType AccountLevyStatus => Db.Object.Accounts.First().LevyStatus;
}
