using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class ApprenticeshipStatisticsControllerTests
    {
        private int _lastNumberOfDays = 30;
        private Mock<IMediator> _mediatorMock;
        private Mock<IModelMapper> _modelMapperMock;
        private Mock<GetApprenticeshipStatisticsQueryResult> _getApprenticeshipStatisticsQueryResult;
        private Mock<GetApprenticeshipStatisticsResponse> _getApprenticeshipStatisticsResponse;
        private ApprenticeshipStatisticsController _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _modelMapperMock = new Mock<IModelMapper>();
            _getApprenticeshipStatisticsQueryResult = new Mock<GetApprenticeshipStatisticsQueryResult>();
            _getApprenticeshipStatisticsResponse = new Mock<GetApprenticeshipStatisticsResponse>();

            _sut = new ApprenticeshipStatisticsController(_mediatorMock.Object, _modelMapperMock.Object);

            _mediatorMock
                .Setup(x => x.Send(It.Is<GetApprenticeshipStatisticsQuery>(x => x.LastNumberOfDays == _lastNumberOfDays),
                    CancellationToken.None))
                .ReturnsAsync(_getApprenticeshipStatisticsQueryResult.Object);

            _modelMapperMock
                .Setup(x => x.Map<GetApprenticeshipStatisticsResponse>(_getApprenticeshipStatisticsQueryResult.Object))
                .ReturnsAsync(_getApprenticeshipStatisticsResponse.Object);
        }

        [Test]
        public async Task WhenCallingGetStatistics_ThenSendsMediatorQueryCorrectly()
        {
            //Act
            await _sut.GetStatistics(_lastNumberOfDays);

            //Assert
            _mediatorMock.Verify(x => x.Send(It.Is<GetApprenticeshipStatisticsQuery>(x => x.LastNumberOfDays == _lastNumberOfDays), default), Times.Once);
        }

        [Test]
        public async Task WhenCallingGetStatistics_AndQueryReturnsNull_ThenReturnsNotFound()
        {
            //Arrange
            _mediatorMock
                .Setup(x => x.Send(
                    It.Is<GetApprenticeshipStatisticsQuery>(x => x.LastNumberOfDays == _lastNumberOfDays),
                    CancellationToken.None))
                .Returns(Task.FromResult<GetApprenticeshipStatisticsQueryResult>(null));

            //Act
            var result = await _sut.GetStatistics(_lastNumberOfDays);

            //Assert
            Assert.True(result is NotFoundResult);
        }

        [Test]
        public async Task WhenCallingGetStatistics_ThenMapsResultToResponseDto()
        {
            //Act
            var result = await _sut.GetStatistics(_lastNumberOfDays);

            //Assert
            _modelMapperMock.Verify(x => x.Map<GetApprenticeshipStatisticsResponse>(_getApprenticeshipStatisticsQueryResult.Object));
        }

        [Test]
        public async Task WhenCallingGetStatistics_ThenGeneratesOkResponse()
        {
            //Act
            var result = await _sut.GetStatistics(_lastNumberOfDays) as OkObjectResult;
            var resultResponse = result?.Value as GetApprenticeshipStatisticsResponse;

            //Assert
            result.Should().NotBeNull();
            resultResponse.Should().NotBeNull();
            resultResponse.Paused.Should().Be(_getApprenticeshipStatisticsQueryResult.Object.PausedApprenticeshipCount);
            resultResponse.Stopped.Should().Be(_getApprenticeshipStatisticsQueryResult.Object.StoppedApprenticeshipCount);
            resultResponse.Approved.Should().Be(_getApprenticeshipStatisticsQueryResult.Object.ApprovedApprenticeshipCount);
        }
    }
}