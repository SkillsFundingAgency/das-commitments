using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticesControllerTests
{
    public class WhenGettingApprovedApprenticesFilterValues
    {
        private Mock<IMediator> _mediator;
        private Mock<ILogger<ApprenticesController>> _logger;
        private ApprenticesController _controller;

        [SetUp]
        public void Init()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ApprenticesController>>();

            _controller = new ApprenticesController(_mediator.Object, _logger.Object);
        }

        [Test]
        public async Task GetApprovedApprentices()
        {
            //Arrange
            var providerId = (uint) 10;

            //Act
            await _controller.GetApprovedApprenticesFilterValues(providerId);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprovedApprenticesFilterValuesQuery>(r => r.ProviderId.Equals(providerId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnApprovedApprentices()
        {
            //Arrange
            var providerId = (uint)10;
            var expectedEmployerNames = new[] {"Test 1", "Test 2"};

            _mediator.Setup(m => m.Send(It.Is<GetApprovedApprenticesFilterValuesQuery>(r => r.ProviderId.Equals(providerId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(new GetApprovedApprenticesFilterValuesResponse
                {
                    EmployerNames = expectedEmployerNames
                });

            //Act
            var result = await _controller.GetApprovedApprenticesFilterValues(providerId) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);

            var filterValues = result.Value as GetApprovedApprenticesFilterValuesResponse;

            Assert.IsNotNull(filterValues);
            Assert.IsNotEmpty(filterValues.EmployerNames);
            Assert.AreEqual(expectedEmployerNames, filterValues.EmployerNames);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprovedApprenticesFilterValues(10) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }

    }
}
