using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class PutCocApprovalCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenHandlingNullCommand_ThenShouldThrowNullArgumentException()
    {
        var fixture = new PutCocApprovalCommandHandlerTestsFixture();

        var act = async () => await fixture.Handler.Handle(null, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndThereIsNoPendingExistingApprovalRequest_ThenShouldDomainException()
    {
        var fixture = new PutCocApprovalCommandHandlerTestsFixture();

        var act = async () => await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        await act.Should().ThrowAsync<PendingApprovalNotFoundException>();
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndThereIsAnExistingApprovalRequest_ThenShouldReturnApprovalResult()
    {
        var fixture = new PutCocApprovalCommandHandlerTestsFixture().WithExistingApprovalRequest();

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        result.Should().BeEquivalentTo(fixture.CocApprovalState.ApprovalResult);
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndThereIsAnExistingApprovalRequest_ThenShouldMarkOldRecordsAsSuperseded()
    {
        var fixture = new PutCocApprovalCommandHandlerTestsFixture().WithExistingApprovalRequest();

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        var oldRequest = fixture.DbContext.ApprovalRequests.FirstOrDefault(r => r.LearningKey == fixture.Command.CocApprovalDetails.LearningKey);
        oldRequest.Should().NotBeNull();
        oldRequest.Status.Should().Be(CocApprovalResultStatus.Superseded);
        oldRequest.Updated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndAutoRejectedApprovalRequest_ThenShouldNotifyProvider()
    {
        var fixture = new PutCocApprovalCommandHandlerTestsFixture().WithExistingApprovalRequest().WithAutoRejectedApprovalRequest();

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        result.Should().BeEquivalentTo(fixture.CocApprovalState.ApprovalResult);

        fixture.NotifyProviderService.Verify(x => x.NotifyProvider(It.Is<long>(p => p == fixture.Command.CocApprovalDetails.ProviderId), It.IsAny<long>(), It.IsAny<string>(),It.IsAny<string>()), Times.Once);
    }
}

public class PutCocApprovalCommandHandlerTestsFixture : IDisposable
{
    public Fixture AutoFixture { get; set; }
    public Mock<ICocApprovalRulesEngine> CocApprovalRules { get; set; }
    public ProviderCommitmentsDbContext DbContext { get; set; }
    public IRequestHandler<PutCocApprovalCommand, CocApprovalResult> Handler { get; set; }
    public PutCocApprovalCommand Command { get; set; }
    public CocApprovalState CocApprovalState { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public Mock<INotifyProviderService> NotifyProviderService { get; set; }

    private const string ProviderCommitmentsBaseUrl = "https://approvals";

    public PutCocApprovalCommandHandlerTestsFixture()
    {
        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        AutoFixture.Customizations.Add(new ModelSpecimenBuilder());

        CocApprovalState = AutoFixture.Create<CocApprovalState>();

        DbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        var approvalDetails = AutoFixture.Build<CocApprovalDetails>().Without(c => c.Apprenticeship).Create();
        Command = new PutCocApprovalCommand { CocApprovalDetails = approvalDetails };

        CocApprovalRules = new Mock<ICocApprovalRulesEngine>();
        CocApprovalRules.Setup(x => x.DetermineApprovalState(Command.CocApprovalDetails)).Returns(CocApprovalState);

        NotifyProviderService = new Mock<INotifyProviderService>();

        Handler = new PutCocApprovalCommandHandler(new Lazy<ProviderCommitmentsDbContext>(DbContext), CocApprovalRules.Object, 
            Mock.Of<ILogger<PostCocApprovalCommandHandler>>(),
            NotifyProviderService.Object);
        CancellationToken = new CancellationToken();
    }

    public PutCocApprovalCommandHandlerTestsFixture WithExistingApprovalRequest()
    {
        DbContext.ApprovalRequests.Add(new ApprovalRequest
        {
            LearningKey = Command.CocApprovalDetails.LearningKey,
            Status = CocApprovalResultStatus.Pending
        });
        DbContext.SaveChanges();
        return this;
    }

    public PutCocApprovalCommandHandlerTestsFixture WithAutoRejectedApprovalRequest()
    {
        CocApprovalState.ApprovalResult.Items.FirstOrDefault().Status = CocApprovalItemStatus.AutoRejected;
        return this;
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}
