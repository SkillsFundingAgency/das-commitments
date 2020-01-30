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
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    public class ApprenticeshipFilterControllerTests
    {
        private Mock<IMediator> _mediator;
        private Mock<ILogger<ApprenticeshipController>> _logger;
        private Mock<IModelMapper> _mapper;
        private ApprenticeshipController _controller;

        [SetUp]
        public void Init()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ApprenticeshipController>>();
            _mapper = new Mock<IModelMapper>();

            _controller = new ApprenticeshipController(_mediator.Object, _mapper.Object, _logger.Object);
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
            var expectedResponse = new GetApprenticeshipsFilterValuesQueryResult
            {
                EmployerNames = new[] {"Test 1", "Test 2"},
                CourseNames = new[] {"Test 3", "Test 4"},
                Statuses = new[] { PaymentStatus.Active, PaymentStatus.Completed },
                StartDates = new[] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2) },
                EndDates = new[] { DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-4) }
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsFilterValuesQuery>(r => r.ProviderId.Equals(providerId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(providerId) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);

            var filterValues = result.Value as GetApprenticeshipsFilterValuesQueryResult;

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
