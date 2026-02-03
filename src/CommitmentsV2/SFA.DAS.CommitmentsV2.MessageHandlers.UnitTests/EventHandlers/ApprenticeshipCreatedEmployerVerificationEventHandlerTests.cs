using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
[Parallelizable]
public class ApprenticeshipCreatedEmployerVerificationEventHandlerTests
{
    [Test]
    public async Task Handle_WhenNoExistingEmployerVerificationRequest_ThenCreatesAndSaves()
    {
        // Arrange
        var fixture = new ApprenticeshipCreatedEmployerVerificationEventHandlerTestsFixture();

        // Act
        await fixture.Handle();

        // Assert
        var saved = await fixture.Db.EmployerVerificationRequests.FindAsync(fixture.Message.ApprenticeshipId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(EmployerVerificationRequestStatus.Pending);
    }

    [Test]
    public async Task Handle_WhenEmployerVerificationRequestAlreadyExists_ThenDoesNotAddDuplicate()
    {
        // Arrange
        var fixture = new ApprenticeshipCreatedEmployerVerificationEventHandlerTestsFixture()
            .WithExistingRequest();

        // Act
        await fixture.Handle();

        // Assert
        var count = fixture.Db.EmployerVerificationRequests.Count(e => e.ApprenticeshipId == fixture.Message.ApprenticeshipId);
        count.Should().Be(1);
    }
}

public class ApprenticeshipCreatedEmployerVerificationEventHandlerTestsFixture
{
    private readonly Fixture _autoFixture;
    public ProviderCommitmentsDbContext Db { get; }
    public ApprenticeshipCreatedEvent Message { get; }
    public ApprenticeshipCreatedEmployerVerificationEventHandler Handler { get; }

    public ApprenticeshipCreatedEmployerVerificationEventHandlerTestsFixture()
    {
        _autoFixture = new Fixture();
        Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        Message = _autoFixture.Build<ApprenticeshipCreatedEvent>()
            .With(m => m.ApprenticeshipId, 12345L)
            .Create();

        Handler = new ApprenticeshipCreatedEmployerVerificationEventHandler(
            new Lazy<ProviderCommitmentsDbContext>(() => Db),
            Mock.Of<ILogger<ApprenticeshipCreatedEmployerVerificationEventHandler>>());
    }

    public Task Handle()
    {
        return Handler.Handle(Message, Mock.Of<IMessageHandlerContext>());
    }

    public ApprenticeshipCreatedEmployerVerificationEventHandlerTestsFixture WithExistingRequest()
    {
        Db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
        {
            ApprenticeshipId = Message.ApprenticeshipId,
            Created = DateTime.UtcNow.AddDays(-1),
            Status = EmployerVerificationRequestStatus.Pending
        });
        Db.SaveChanges();
        return this;
    }
}
