using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Queries.GetStatistics;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.StatisticsControllerTests
{
    [TestFixture]
    public class WhenGettingStatistics
    {
        private Mock<IMediator> _mockMediator;
        private Mock<IStatisticsMapper> _statisticsMapper;
        private StatisticsController _controller;
        private StatisticsOrchestrator _statisticsOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _statisticsMapper = new Mock<IStatisticsMapper>();
            _statisticsMapper.Setup(x => x.MapFrom(It.IsAny<Statistics>())).Returns(new ConsistencyStatistics());

            _statisticsOrchestrator = new StatisticsOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(),
                _statisticsMapper.Object);

            _controller = new StatisticsController(_statisticsOrchestrator);
        }

        [Test]
        public async Task ThenTheMediatorIsCalled()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetStatisticsRequest>()))
                .ReturnsAsync(new GetStatisticsResponse());

            await _controller.GetStatistics();

            _mockMediator.Verify(x => x.SendAsync(It.IsAny<GetStatisticsRequest>()));
        }

        [Test]
        public void ThenReturnsABadResponseIfMediatorThrowsAInvalidRequestException()
        {
            const string errorMessage = "Error message";

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetStatisticsRequest>()))
                .ThrowsAsync(new ValidationException(errorMessage));

            var validationException =
                Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetStatistics());

            Assert.That(validationException.Message, Is.EqualTo(errorMessage));
        }

        [Test, AutoData]
        public async Task ThenReturnsStatistics(GetStatisticsResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetStatisticsRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetStatistics() as OkNegotiatedContentResult<ConsistencyStatistics>;

            result?.Content.Should().NotBeNull();

            _statisticsMapper.Verify(x => x.MapFrom(It.IsAny<Statistics>()), Times.Once);
        }
    }
}
