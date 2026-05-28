using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistory;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers;

public class ChangeHistoryControllerTests
{
    private Mock<IMediator> _mediator;
    private ChangeHistoryController _controller;
    private Mock<ILogger<ChangeHistoryController>> _logger;

    [SetUp]
    public void Init()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<ChangeHistoryController>>();
        _controller = new ChangeHistoryController(_mediator.Object, _logger.Object);
    }

    [Test, MoqAutoData]
    public async Task GetChangeHistory_Then_ReturnValidResponse(
        GetChangeHistoryQueryResult changeHistoryresult,
        long apprenticeshipId)
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.Is<GetChangeHistoryQuery>(t => t.ApprenticeshipId == apprenticeshipId)))
            .ReturnsAsync(changeHistoryresult);

        // Act
        var result = await _controller.GetChangeHistory(apprenticeshipId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var jsonResult = result as OkObjectResult;
        var getChangeHistoryResponse = jsonResult?.Value as GetChangeHistoryResponse;
        getChangeHistoryResponse.ChangeHistory.Should().HaveCount(changeHistoryresult.ChangeHistory.Count);
        getChangeHistoryResponse.ChangeHistory.Should().BeEquivalentTo(changeHistoryresult.ChangeHistory);
    }

    [Test, MoqAutoData]
    public async Task GetChangeHistory_Then_ReturnNotFound(long apprenticeshipId)
    {
        {
            // Arrange
            _mediator.Setup(m => m.Send(It.Is<GetChangeHistoryQuery>(t => t.ApprenticeshipId == apprenticeshipId)))
                .ReturnsAsync((GetChangeHistoryQueryResult)null);

            // Act
            var result = await _controller.GetChangeHistory(apprenticeshipId) as NotFoundResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
        }
    }
}