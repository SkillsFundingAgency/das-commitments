using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using SFA.DAS.Commitments.Application.Commands.TriageDataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    [TestFixture]
    public class WhenUpdatingDataLock
    {
        private ApprenticeshipsController _controller;
        private ApprenticeshipsOrchestrator _orchestrator;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void Arrange()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<TriageDataLockCommand>()))
                .ReturnsAsync(new Unit());

            _orchestrator = new ApprenticeshipsOrchestrator(
                _mockMediator.Object,
                Mock.Of<IDataLockMapper>(),
                Mock.Of<IApprenticeshipMapper>(),
                Mock.Of<ICommitmentsLogger>());

            _controller = new ApprenticeshipsController(_orchestrator);
        }

        [Test]
        public async Task ThenTheMediatorIsCalled()
        {
            //Arrange
            var payload = new DataLockTriageSubmission
            {
                TriageStatus = TriageStatus.Restart,
                UserId = "USER"
            };

            //Act
            await _controller.PatchDataLock(1, 2, payload);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(
                It.Is<TriageDataLockCommand>(
                    t => t.TriageStatus == Domain.Entities.DataLock.TriageStatus.Restart
                    && t.UserId == "USER"
                    && t.ApprenticeshipId == 1
                    && t.DataLockEventId == 2
                    ))
                , Times.Once);
        }
    }
}
