using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.PauseApprenticeship;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
public class PauseApprenticeshipCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenPausingLiveLearner_RecordsChangeHistory()
    {
        var appliedDate = new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc);
        var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), db => db.EnableNullChecks(false))
            .Options;
        await using var dbContext = new ProviderCommitmentsDbContext(options);
        var apprenticeship = new Apprenticeship
        {
            Cohort = new Cohort { AccountLegalEntity = new AccountLegalEntity() },
            PaymentStatus = PaymentStatus.Active,
            StartDate = appliedDate.AddMonths(-2)
        };
        dbContext.Apprenticeships.Add(apprenticeship);
        await dbContext.SaveChangesAsync();
        _ = new UnitOfWorkContext();

        var messageSession = new Mock<IMessageSession>();
        var handler = new PauseApprenticeshipCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => dbContext),
            Mock.Of<ICurrentDateTime>(clock => clock.UtcNow == appliedDate),
            Mock.Of<IAuthenticationService>(authentication => authentication.GetUserParty() == Party.Employer),
            messageSession.Object);
        var userId = Guid.NewGuid();
        var command = new PauseApprenticeshipCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            UserInfo = new UserInfo { UserId = userId.ToString() }
        };

        await handler.Handle(command, CancellationToken.None);

        messageSession.Verify(session => session.Send(It.Is<StoreLearningHistoryCommand>(history =>
            history.ApprenticeshipId == apprenticeship.Id &&
            history.AppliedDate == appliedDate &&
            history.Source == LearningSourceType.ManualChange &&
            history.ChangeType == LearningChangeType.ManualUpdate &&
            history.UserId == userId &&
            history.Description == "Status change from live to paused"), It.IsAny<SendOptions>()), Times.Once);
    }
}
