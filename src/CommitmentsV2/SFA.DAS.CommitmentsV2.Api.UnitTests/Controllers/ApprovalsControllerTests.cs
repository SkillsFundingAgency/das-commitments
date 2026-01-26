using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
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
        public async Task PostApprovals_Processes_The_Request_Then_ReturnsResponse()
        {
            // Arrange
            var request = _fixture.Create<CocApprovalRequest>();
            var command = _fixture.Build<PostCocApprovalCommand>().Without(m=>m.Apprenticeship).Create();
            var commandResult = _fixture.Create<CocApprovalResult>();

            _mapper.Setup(m => m.Map<PostCocApprovalCommand>(request)).ReturnsAsync(command);
            _mediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);

            // Act
            var result = await _controller.PostApprovals(Guid.NewGuid(), request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var jsonResult = result as OkObjectResult;
            jsonResult.StatusCode.Should().Be(200);
            jsonResult.Value.Should().BeEquivalentTo(commandResult.Items);
        }
    }
}
