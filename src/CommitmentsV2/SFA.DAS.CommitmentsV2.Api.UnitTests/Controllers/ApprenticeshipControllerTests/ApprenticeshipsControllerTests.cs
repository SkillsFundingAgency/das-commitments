using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;
using ApprenticeshipDetailsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse.ApprenticeshipDetailsResponse;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    public class ApprenticeshipsControllerTests
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
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10
            };

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                    r.ProviderId.Equals(request.ProviderId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetApprovedApprenticesByPage()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
                PageNumber = 4,
                PageItemCount = 17
            };

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                    r.ProviderId.Equals(request.ProviderId) &&
                    r.PageNumber.Equals(request.PageNumber) &&
                    r.PageItemCount.Equals(request.PageItemCount)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnApprovedApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10
            };
            const int expectedTotalApprenticeshipsFound = 10;
            const int expectedTotalApprenticeshipsWithAlertsFound = 3;

            var expectedApprenticeship = new ApprenticeshipDetailsResponse
            {
                Id = new Fixture().Create<long>(),
                FirstName = "George",
                LastName = "Test",
                Uln = "12345",
                EmployerName = "Test Corp",
                CourseName = "Testing Level 1",
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddMonths(2),
                PaymentStatus = PaymentStatus.Active,
                Alerts = new []{Alerts.IlrDataMismatch, Alerts.ChangesForReview}
            };

            _mapper.Setup(x =>
                    x.Map<ApprenticeshipDetailsResponse>(It.IsAny<ApprenticeshipDetails>()))
                .ReturnsAsync(expectedApprenticeship);

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsQuery>(r => r.ProviderId.Equals(request.ProviderId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(new GetApprenticeshipsQueryResponse
            {
                    Apprenticeships = new []{ new ApprenticeshipDetails() },
                    TotalApprenticeshipsFound = expectedTotalApprenticeshipsFound,
                    TotalApprenticeshipsWithAlertsFound = expectedTotalApprenticeshipsWithAlertsFound
            });

            //Act
            var result = await _controller.GetApprenticeships(request) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);

            var response = result.Value as GetApprenticeshipsResponse;

            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response.Apprenticeships);

            var actualApprenticeship = response.Apprenticeships.First();

            Assert.AreEqual(expectedApprenticeship, actualApprenticeship);
            Assert.AreEqual(expectedTotalApprenticeshipsFound, response.TotalApprenticeshipsFound);
            Assert.AreEqual(expectedTotalApprenticeshipsWithAlertsFound, response.TotalApprenticeshipsWithAlertsFound);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprenticeships(new GetApprenticeshipsRequest()) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }
    }
}
