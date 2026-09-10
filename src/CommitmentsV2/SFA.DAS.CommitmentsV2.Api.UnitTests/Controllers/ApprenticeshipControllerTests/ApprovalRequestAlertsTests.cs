using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprovalRequestAlertSeen;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovalRequest;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests;

public class ApprovalRequestAlertsTests
{
    private Mock<IMediator> _mediator;
    private ApprenticeshipController _controller;
    private Mock<IModelMapper> _modelMapper;
    private Mock<IAuthenticationService> _authenticationService;
    private Mock<ILogger<ApprenticeshipController>> _logger;

    [SetUp]
    public void Init()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<ApprenticeshipController>>();
        _modelMapper = new Mock<IModelMapper>();
        _authenticationService = new Mock<IAuthenticationService>();
        _controller = new ApprenticeshipController(_mediator.Object, _modelMapper.Object, _authenticationService.Object, _logger.Object);
    }

    [Test, MoqAutoData]
    public async Task GetApprovalRequest_Then_ReturnValidResponse(
        GetApprovalRequestQueryResult approvalRequestresult,
        long apprenticeshipId,
        long accountId)
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.Is<GetApprovalRequestQuery>(t => t.ApprenticeshipId == apprenticeshipId && t.AccountId == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalRequestresult);

        // Act
        var result = await _controller.GetApprovalRequestsForApprenticeship(apprenticeshipId, (byte)CocApprovalItemStatus.AutoApproved, accountId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var jsonResult = result as OkObjectResult;
        var getApprovalRequestResponse = jsonResult?.Value as GetApprovalRequestQueryResponse;
        getApprovalRequestResponse.ApprovalRequests.Should().HaveCount(approvalRequestresult.ApprovalRequests.Count);
        getApprovalRequestResponse.ApprovalRequests.Should().BeEquivalentTo(approvalRequestresult.ApprovalRequests);
        getApprovalRequestResponse.ApprenticeName.Should().BeEquivalentTo(approvalRequestresult.ApprenticeName);
    }

    [Test, MoqAutoData]
    public async Task GetApprovalRequest_Then_ReturnEmptyResponse(long apprenticeshipId, long accountId)
    {
        {
            // Arrange
            _mediator.Setup(m => m.Send(It.Is<GetApprovalRequestQuery>(t => t.ApprenticeshipId == apprenticeshipId && t.AccountId == accountId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApprovalRequestQueryResult() { ApprovalRequests = [] });

            // Act
            var result = await _controller.GetApprovalRequestsForApprenticeship(apprenticeshipId, (byte)CocApprovalItemStatus.AutoApproved, accountId) as ObjectResult;
            var model = result?.Value as GetApprovalRequestQueryResponse;

            // Assert
            model.Should().NotBeNull();
            model.ApprovalRequests.Should().BeEmpty();
        }
    }

    [Test, MoqAutoData]
    public async Task UpdateApprovalRequestAlert_Then_ReturnValidResponse(
       ApprovalRequestUpdateAlertAcknowledge request,

       long apprenticeshipId)
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.Is<UpdateApprovalRequestAlertAcknowledgeCommand>(t => t.ApprenticeshipId == apprenticeshipId), It.IsAny<CancellationToken>()));

        // Act
        var result = await _controller.UpdateApprovalRequestAlertAcknowledge(apprenticeshipId, request) as OkObjectResult;

        // Assert
        _mediator.Verify(p => p.Send(It.Is<UpdateApprovalRequestAlertAcknowledgeCommand>(c =>
                   c.ApprenticeshipId == apprenticeshipId && c.ApprovalRequests.All(a => request.ApprovalRequestAlerts.Any(
                       b => b.ApprovalRequestId == a.ApprovalRequestId
                   && a.UserInfo == b.UserInfo))),
                   It.IsAny<CancellationToken>()), Times.Once);
    }
}