using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    public class ApprenticeshipFilterControllerTests
    {
        private Mock<IMediator> _mediator;
        private Mock<ILogger<ApprenticeshipsController>> _logger;
        private ApprenticeshipsController _controller;

        [SetUp]
        public void Init()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ApprenticeshipsController>>();

            _controller = new ApprenticeshipsController(_mediator.Object, _logger.Object);
        }

        [Test]
        public async Task GetApprovedApprentices()
        {
            //Arrange
            var providerId = 10;

            //Act
            await _controller.GetApprenticeshipsFilterValues(providerId);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsFilterValuesQuery>(r => r.ProviderId.Equals(providerId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnApprovedApprentices()
        {
            //Arrange
            var providerId = 10;
            var expectedResponse = new GetApprenticeshipsFilterValuesResponse
            {
                EmployerNames = new[] {"Test 1", "Test 2"},
                CourseNames = new[] {"Test 3", "Test 4"},
                Statuses = new[] { "Test 5", "Test 6" },
                StartDates = new[] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2) },
                EndDates = new[] { DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-4) }
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsFilterValuesQuery>(r => r.ProviderId.Equals(providerId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(providerId) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);

            var filterValues = result.Value as Types.Responses.GetApprenticeshipsFilterValuesResponse;

            filterValues.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(10) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }

    }
}
