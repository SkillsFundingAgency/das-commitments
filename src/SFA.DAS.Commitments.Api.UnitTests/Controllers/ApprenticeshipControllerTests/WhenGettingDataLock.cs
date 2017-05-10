using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Application.Queries.GetDataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    [TestFixture]
    public class WhenGettingDataLock
    {
        private ApprenticeshipsController _controller;
        private ApprenticeshipsOrchestrator _orchestrator;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void Arrange()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetDataLockRequest>()))
                .ReturnsAsync(new GetDataLockResponse());


            _orchestrator = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>());
            _controller = new ApprenticeshipsController(_orchestrator);
        }

        [Test]
        public async Task ThenTheMediatorIsCalled()
        {
            //Act
            await _controller.GetDataLock(1,2);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(
                It.Is<GetDataLockRequest>(r => r.ApprenticeshipId == 1 && r.DataLockEventId == 2)),
                Times.Once);
        }
    }
}
