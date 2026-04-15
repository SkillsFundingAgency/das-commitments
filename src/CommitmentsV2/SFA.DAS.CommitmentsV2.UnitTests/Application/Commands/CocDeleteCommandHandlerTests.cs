using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocDelete;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class CocDeleteCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenHandlingNullCommand_ThenShouldThrowNullArgumentException()
    {
        var fixture = new CocDeleteCommandHandlerTestsFixture();

        var act = async () => await fixture.Handler.Handle(null, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndThereIsACancelledExistingApprovalRequest_ThenShouldReturnNotPending()
    {
        var fixture = new CocDeleteCommandHandlerTestsFixture().WithExistingApprovalRequest(CocApprovalResultStatus.Cancelled);

        var act = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        act.Status.Should().Be(DeleteValidationState.NotPending);
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndThereIsAPendingExistingApprovalRequest_ThenShouldReturnCancelled()
    {
        var fixture = new CocDeleteCommandHandlerTestsFixture().WithExistingApprovalRequest(CocApprovalResultStatus.Pending);

        var act = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        act.Status.Should().Be(DeleteValidationState.Cancelled);
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndThereIsNoExistingApprovalRequest_ThenShouldReturnNotFound()
    {
        var fixture = new CocDeleteCommandHandlerTestsFixture();

        var act = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        act.Status.Should().Be(DeleteValidationState.NotFound);
    }
}

public class CocDeleteCommandHandlerTestsFixture : IDisposable
{
    public Fixture AutoFixture { get; set; }
    public ProviderCommitmentsDbContext DbContext { get; set; }
    public IRequestHandler<CocDeleteCommand, CocDeleteResult> Handler { get; set; }
    public CocDeleteCommand Command { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public CocDeleteCommandHandlerTestsFixture()
    {
        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        AutoFixture.Customizations.Add(new ModelSpecimenBuilder());

        DbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        Command = AutoFixture.Build<CocDeleteCommand>().Create();

        Handler = new CocDeleteCommandHandler(new Lazy<ProviderCommitmentsDbContext>(DbContext), Mock.Of<ILogger<CocDeleteCommandHandler>>());
        CancellationToken = new CancellationToken();
    }

    public CocDeleteCommandHandlerTestsFixture WithExistingApprovalRequest(CocApprovalResultStatus status)
    {
        DbContext.ApprovalRequests.Add(new ApprovalRequest
        {
            LearningKey = Command.LearningKey,
            Status = status,
            Created = DateTime.UtcNow.AddHours(-1)
        });
        DbContext.SaveChanges();
        return this;
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}