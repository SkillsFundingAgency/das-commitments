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
        public async Task GetProviderApprovedApprentices()
        {
            //Arrange
            var providerId = 10;

            //Act
            await _controller.GetApprenticeshipsFilterValues(providerId, null);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsFilterValuesQuery>(r =>  r.ProviderId.HasValue && r.ProviderId.Value.Equals(providerId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task GetEmployerApprovedApprentices()
        {
            //Arrange
            var employerAccountId = 10;

            //Act
            await _controller.GetApprenticeshipsFilterValues(null, employerAccountId);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsFilterValuesQuery>(r =>  r.EmployerAccountId.HasValue && r.EmployerAccountId.Value.Equals(employerAccountId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnProviderApprovedApprentices()
        {
            //Arrange
            var providerId = 10;
            var expectedResponse = new GetApprenticeshipsFilterValuesQueryResult
            {
                EmployerNames = new[] {"Test 1", "Test 2"},
                CourseNames = new[] {"Test 3", "Test 4"},
                StartDates = new[] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2) },
                EndDates = new[] { DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-4) }
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsFilterValuesQuery>(r =>  r.ProviderId.HasValue && r.ProviderId.Value.Equals(providerId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(providerId, null) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);

            var filterValues = result.Value as GetApprenticeshipsFilterValuesQueryResult;

            filterValues.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task ReturnEmployerApprovedApprentices()
        {
            //Arrange
            var employerAccountId = 10;
            var expectedResponse = new GetApprenticeshipsFilterValuesQueryResult
            {
                EmployerNames = new[] {"Test 1", "Test 2"},
                CourseNames = new[] {"Test 3", "Test 4"},
                StartDates = new[] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2) },
                EndDates = new[] { DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-4) }
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsFilterValuesQuery>(r =>  r.EmployerAccountId.HasValue && r.EmployerAccountId.Value.Equals(employerAccountId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(null, employerAccountId) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);

            var filterValues = result.Value as GetApprenticeshipsFilterValuesQueryResult;

            filterValues.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(10, null) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }

    }
}
