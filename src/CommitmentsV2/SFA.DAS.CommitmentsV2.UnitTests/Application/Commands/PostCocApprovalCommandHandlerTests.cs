using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class PostCocApprovalCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenHandlingNullCommand_ThenShouldThrowNullArgumentException()
    {
        var fixture = new PostCocApprovalCommandHandlerTestsFixture();

        var act = async () => await fixture.Handler.Handle(null, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndThereIsAPendingExistingApprovalRequest_ThenShouldDomainException()
    {
        var fixture = new PostCocApprovalCommandHandlerTestsFixture().WithExistingApprovalRequest();

        var act = async () => await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Test]
    public async Task Handle_WhenHandlingCommandAndNoExistingApprovalRequest_ThenShouldReturnApprovalResult()
    {
        var fixture = new PostCocApprovalCommandHandlerTestsFixture();

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        result.Should().BeEquivalentTo(fixture.CocApprovalState.ApprovalResult);
    }
}

public class PostCocApprovalCommandHandlerTestsFixture : IDisposable
{
    public Fixture AutoFixture { get; set; }
    public Mock<ICocApprovalRulesEngine> CocApprovalRules { get; set; }
    public ProviderCommitmentsDbContext DbContext { get; set; }
    public IRequestHandler<PostCocApprovalCommand, CocApprovalResult> Handler { get; set; }
    public PostCocApprovalCommand Command { get; set; }
    public CocApprovalState CocApprovalState { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public PostCocApprovalCommandHandlerTestsFixture()
    {
        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        AutoFixture.Customizations.Add(new ModelSpecimenBuilder());

        CocApprovalState = AutoFixture.Create<CocApprovalState>();

        DbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        var approvalDetails = AutoFixture.Build<CocApprovalDetails>().Without(c => c.Apprenticeship).Create();
        Command = new PostCocApprovalCommand { CocApprovalDetails = approvalDetails };

        CocApprovalRules = new Mock<ICocApprovalRulesEngine>();
        CocApprovalRules.Setup(x => x.DetermineApprovalState(Command.CocApprovalDetails)).Returns(CocApprovalState);

        Handler = new PostCocApprovalCommandHandler(new Lazy<ProviderCommitmentsDbContext>(DbContext), CocApprovalRules.Object, Mock.Of<ILogger<PostCocApprovalCommandHandler>>());
        CancellationToken = new CancellationToken();
    }

    public PostCocApprovalCommandHandlerTestsFixture WithExistingApprovalRequest()
    {
        DbContext.ApprovalRequests.Add(new ApprovalRequest
        {
            LearningKey = Command.CocApprovalDetails.LearningKey,
            Status = CocApprovalResultStatus.Pending
        });
        DbContext.SaveChanges();
        return this;
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}
