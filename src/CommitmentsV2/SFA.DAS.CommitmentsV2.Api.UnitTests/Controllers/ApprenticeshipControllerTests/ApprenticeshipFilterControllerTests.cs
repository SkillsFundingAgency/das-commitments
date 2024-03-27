using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
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
        public async Task GetProviderApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipFiltersRequest
            {
                ProviderId = 10
            };

            //Act
            await _controller.GetApprenticeshipsFilterValues(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsFilterValuesQuery>(r =>  r.ProviderId.HasValue && r.ProviderId.Value.Equals(request.ProviderId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task GetEmployerApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipFiltersRequest
            {
                EmployerAccountId = 10
            };

            //Act
            await _controller.GetApprenticeshipsFilterValues(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsFilterValuesQuery>(r =>  r.EmployerAccountId.HasValue && r.EmployerAccountId.Value.Equals(request.EmployerAccountId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnProviderApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipFiltersRequest
            {
                ProviderId = 10
            };

            var expectedResponse = new GetApprenticeshipsFilterValuesQueryResult
            {
                EmployerNames = new[] {"Test 1", "Test 2"},
                CourseNames = new[] {"Test 3", "Test 4"},
                StartDates = new[] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2) },
                EndDates = new[] { DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-4) }
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsFilterValuesQuery>(r =>  r.ProviderId.HasValue && r.ProviderId.Value.Equals(request.ProviderId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(request) as OkObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null);

            var filterValues = result.Value as GetApprenticeshipsFilterValuesQueryResult;

            filterValues.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task ReturnEmployerApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipFiltersRequest
            {
                EmployerAccountId = 10
            };

            var expectedResponse = new GetApprenticeshipsFilterValuesQueryResult
            {
                EmployerNames = new[] {"Test 1", "Test 2"},
                CourseNames = new[] {"Test 3", "Test 4"},
                StartDates = new[] { DateTime.Now.AddDays(-4), DateTime.Now.AddDays(-3) },
                EndDates = new[] { DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1) }
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsFilterValuesQuery>(r =>  r.EmployerAccountId.HasValue && r.EmployerAccountId.Value.Equals(request.EmployerAccountId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(request) as OkObjectResult;

            //Assert
            Assert.That(result, Is.Not.Null);

            var filterValues = result.Value as GetApprenticeshipsFilterValuesQueryResult;

            filterValues.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Arrange
            var request = new GetApprenticeshipFiltersRequest
            {
                ProviderId = 10
            };

            //Act
            var result = await _controller.GetApprenticeshipsFilterValues(request) as NotFoundResult;

            //Assert
            Assert.That(result, Is.Not.Null);
        }

    }
}
