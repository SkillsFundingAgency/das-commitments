using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Application.Queries.GetDataLocks;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    [TestFixture]
    public class WhenGettingDataLocks
    {
        private ApprenticeshipsController _controller;
        private ApprenticeshipsOrchestrator _orchestrator;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void Arrange()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetDataLocksRequest>()))
                .ReturnsAsync(new GetDataLocksResponse());

            _orchestrator = new ApprenticeshipsOrchestrator(_mockMediator.Object);
            _controller = new ApprenticeshipsController(_orchestrator);
        }

        [Test]
        public async Task ThenTheMediatorIsCalled()
        {
            //Act
            await _controller.GetDataLocks(1);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(
                It.Is<GetDataLocksRequest>(r => r.ApprenticeshipId == 1)),
                Times.Once);
        }
    }
}
