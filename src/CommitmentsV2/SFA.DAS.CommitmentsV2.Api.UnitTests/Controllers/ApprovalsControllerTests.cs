using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class ApprovalsControllerTests
{
    private Fixture _fixture;
    private Mock<IMediator> _mediator;
    private Mock<IModelMapper> _mapper;
    private ApprovalsController _controller;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mediator = new Mock<IMediator>();
        _mapper = new Mock<IModelMapper>();
        _controller = new ApprovalsController(_mediator.Object, _mapper.Object, Mock.Of<ILogger<ApprovalsController>>());
    }

    [Test]
    public async Task CocApprovals_Processes_A_New_Request_Then_ReturnsResponse()
    {
        // Arrange
        var request = _fixture.Create<CocApprovalRequest>();
        var cocApprovalDetails = _fixture.Build<CocApprovalDetails>().Without(x => x.Apprenticeship).Create();
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>().With(x => x.CocApprovalDetails, cocApprovalDetails).With(x=>x.Action, AggregrationAction.CreateNew).Create();
        var commandResult = _fixture.Create<CocApprovalResult>();

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);
        commandResult.Items.Where(x=>x.Status == CocApprovalItemStatus.Pending).ToList().ForEach(i => i.Status = CocApprovalItemStatus.AutoApproved);

        // Act
        var result = await _controller.PostApprovals(request.LearningKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CreatedResult>();
        var jsonResult = result as CreatedResult;
        jsonResult.StatusCode.Should().Be(201);
        jsonResult.Value.Should().BeEquivalentTo(commandResult.Items.Select(x => new { ChangeType = x.Field.GetEnumDescription(), ApprovalStatus = x.Status.GetEnumDescription(), x.Reason }).ToList());
    }

    [Test]
    public async Task PostApprovals_Returns_BadRequest_When_LearningKey_Not_Matching()
    {
        // Arrange
        var request = _fixture.Create<CocApprovalRequest>();

        // Act
        var result = await _controller.PostApprovals(Guid.NewGuid(), request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<BadRequestObjectResult>();
        var jsonResult = result as BadRequestObjectResult;
        jsonResult.StatusCode.Should().Be(400);
    }

    [Test]
    public async Task CocApprovals_Processes_A_Request_And_Pending_Request_Exists_Then_ReturnsResponse()
    {
        var request = _fixture.Create<CocApprovalRequest>();
        var cocApprovalDetails = _fixture.Build<CocApprovalDetails>().Without(x => x.Apprenticeship).Create();
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>()
            .With(x => x.CocApprovalDetails, cocApprovalDetails)
            .With(x=>x.PreviousApprovalRequestId, Guid.NewGuid())
            .With(x => x.Action, AggregrationAction.SupersedePrevious).Create();
        var commandResult = _fixture.Create<CocApprovalResult>();

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);
        commandResult.Items.ForEach(i => i.Status = CocApprovalItemStatus.Pending);

        // Act
        var result = await _controller.PostApprovals(request.LearningKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CreatedResult>();
        var jsonResult = result as CreatedResult;
        jsonResult.StatusCode.Should().Be(201);
        jsonResult.Value.Should().BeEquivalentTo(commandResult.Items.Select(x => new { ChangeType = x.Field.GetEnumDescription(), ApprovalStatus = "EmployerApprovalRequested", x.Reason }).ToList());
    }

    [Test]
    public async Task CocApprovals_Processes_A_Request_Which_Cancels_Pending_Request_Then_ReturnsResponse()
    {
        var request = _fixture.Create<CocApprovalRequest>();
        var cocApprovalCommand = _fixture.Build<CocApprovalCommand>()
            .Without(x => x.CocApprovalDetails)
            .With(x => x.PreviousApprovalRequestId, Guid.NewGuid())
            .With(x => x.Action, AggregrationAction.CancelPrevious).Create();
        var commandResult = _fixture.Create<CocApprovalResult>();

        _mapper.Setup(m => m.Map<CocApprovalCommand>(request)).ReturnsAsync(cocApprovalCommand);
        _mediator.Setup(m => m.Send(cocApprovalCommand, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);

        // Act
        var result = await _controller.PostApprovals(request.LearningKey, request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var jsonResult = result as OkObjectResult;
        jsonResult.StatusCode.Should().Be(200);
        jsonResult.Value.Should().BeEquivalentTo(request.Changes.Select(x => new { x.ChangeType, ApprovalStatus = "AutoApproved", Reason = (string)null }).ToList());
    }
}

