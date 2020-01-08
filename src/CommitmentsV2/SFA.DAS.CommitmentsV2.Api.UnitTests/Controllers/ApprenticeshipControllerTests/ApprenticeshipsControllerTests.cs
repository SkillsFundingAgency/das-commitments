using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Types;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;


namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    public class ApprenticeshipsControllerTests
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
            const uint providerId = 10;

            //Act
            await _controller.GetApprenticeships(providerId);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsRequest>(r => 
                    r.ProviderId.Equals(providerId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetApprovedApprenticesByPage()
        {
            //Arrange
            const uint expectedProviderId = 10;
            const int expectedPageNumber = 4;
            const int expectedPageItemCount = 17;

            //Act
            await _controller.GetApprenticeships(expectedProviderId, expectedPageNumber, expectedPageItemCount);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsRequest>(r => 
                    r.ProviderId.Equals(expectedProviderId) &&
                    r.PageNumber.Equals(expectedPageNumber) &&
                    r.PageItemCount.Equals(expectedPageItemCount)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnApprovedApprentices()
        {
            //Arrange
            const uint providerId = 10;
            const int expectedTotalApprenticeshipsFound = 10;
            const int expectedTotalApprenticeshipsWithAlertsFound = 3;

            var expectedApprenticeship = new ApprenticeshipDetails
            {
                ApprenticeFirstName = "George",
                ApprenticeLastName = "Test",
                Uln = "12345",
                EmployerName = "Test Corp",
                CourseName = "Testing Level 1",
                PlannedStartDate = DateTime.Now.AddDays(2),
                PlannedEndDateTime = DateTime.Now.AddMonths(2),
                PaymentStatus = PaymentStatus.Active,
                Alerts = new []{"one", "two"}
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsRequest>(r => r.ProviderId.Equals(providerId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(new Application.Queries.GetApprenticeships.GetApprenticeshipsResponse
            {
                    Apprenticeships = new []{ expectedApprenticeship},
                    TotalApprenticeshipsFound = expectedTotalApprenticeshipsFound,
                    TotalApprenticeshipsWithAlertsFound = expectedTotalApprenticeshipsWithAlertsFound
            });

            //Act
            var result = await _controller.GetApprenticeships(providerId) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);

            var response = result.Value as GetApprenticeshipsResponse;

            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response.Apprenticeships);

            var actualApprenticeship = response.Apprenticeships.First();

            Assert.AreEqual(expectedApprenticeship.ApprenticeFirstName, actualApprenticeship.ApprenticeFirstName);
            Assert.AreEqual(expectedApprenticeship.ApprenticeLastName, actualApprenticeship.ApprenticeLastName);
            Assert.AreEqual(expectedApprenticeship.Uln, actualApprenticeship.Uln);
            Assert.AreEqual(expectedApprenticeship.EmployerName, actualApprenticeship.EmployerName);
            Assert.AreEqual(expectedApprenticeship.CourseName, actualApprenticeship.CourseName);
            Assert.AreEqual(expectedApprenticeship.PlannedStartDate, actualApprenticeship.PlannedStartDate);
            Assert.AreEqual(expectedApprenticeship.PlannedEndDateTime, actualApprenticeship.PlannedEndDateTime);
            Assert.AreEqual(expectedApprenticeship.PaymentStatus, actualApprenticeship.PaymentStatus);
            Assert.AreEqual(expectedApprenticeship.Alerts, actualApprenticeship.Alerts);
            
            Assert.AreEqual(expectedTotalApprenticeshipsFound, response.TotalApprenticeshipsFound);
            Assert.AreEqual(expectedTotalApprenticeshipsWithAlertsFound, response.TotalApprenticeshipsWithAlertsFound);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprenticeships(10) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }

    }
}
