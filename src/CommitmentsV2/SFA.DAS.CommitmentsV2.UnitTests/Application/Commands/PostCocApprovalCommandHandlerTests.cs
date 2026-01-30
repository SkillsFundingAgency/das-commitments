using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

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
    public async Task Handle_WhenHandlingCommandAndNoExistingApprovalRequest_ThenShouldThrowNullArgumentException()
    {
        var fixture = new PostCocApprovalCommandHandlerTestsFixture();

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(CocApprovalResultStatus.Pending);
        result.Items.Should().BeEquivalentTo(fixture.CocUpdateStatuses);
    }
}

public class PostCocApprovalCommandHandlerTestsFixture : IDisposable
{
    public Fixture AutoFixture { get; set; }
    public Mock<ICocApprovalService> CocApprovalService { get; set; }
    public ProviderCommitmentsDbContext DbContext { get; set; }
    public IRequestHandler<PostCocApprovalCommand, CocApprovalResult> Handler { get; set; }
    public PostCocApprovalCommand Command { get; set; }
    public List<CocUpdateResult> CocUpdateStatuses { get; set; }
    public List<CocApprovalFieldChange> ApprovalFieldChanges { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public PostCocApprovalCommandHandlerTestsFixture()
    {
        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        AutoFixture.Customizations.Add(new ModelSpecimenBuilder());

        CocUpdateStatuses = new List<CocUpdateResult>
        {
            new CocUpdateResult
            {
                Field = CocChangeField.TNP1,
                Status = CocApprovalItemStatus.Pending
            },
            new CocUpdateResult
            {
                Field = CocChangeField.TNP2,
                Status = CocApprovalItemStatus.Pending
            }
        };

        ApprovalFieldChanges = new List<CocApprovalFieldChange>
        {
            new CocApprovalFieldChange
            {
                ChangeType = "TNP1",
                Data = new CocData
                {
                    Old = "1000",
                    New = "1500"
                }
            },
            new CocApprovalFieldChange
            {
                ChangeType = "TNP2",
                Data = new CocData
                {
                    Old = "2000",
                    New = "1500"
                }
            }
        };

        DbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        Command = AutoFixture.Build<PostCocApprovalCommand>().Without(c => c.Apprenticeship).With(c=>c.ApprovalFieldChanges, ApprovalFieldChanges).Create();
        CocApprovalService = new Mock<ICocApprovalService>();
        CocApprovalService.Setup(x => x.DetermineCocUpdateStatuses(Command.Updates, Command.Apprenticeship)).Returns(CocUpdateStatuses);

        Handler = new PostCocApprovalCommandHandler(new Lazy<ProviderCommitmentsDbContext>(DbContext), CocApprovalService.Object, Mock.Of<ILogger<PostCocApprovalCommandHandler>>());
        CancellationToken = new CancellationToken();
    }

    public PostCocApprovalCommandHandlerTestsFixture WithExistingApprovalRequest()
    {
        DbContext.ApprovalRequests.Add(new ApprovalRequest
        {
            LearningKey = Command.LearningKey,
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
