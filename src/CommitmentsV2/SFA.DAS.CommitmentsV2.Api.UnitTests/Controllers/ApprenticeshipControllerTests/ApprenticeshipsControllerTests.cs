using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;
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
        public async Task GetApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipRequest
            {
                ProviderId = 10
            };

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsRequest>(r => 
                    r.ProviderId.Equals(request.ProviderId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task GetFilterApprenticeships([Frozen]GetApprenticeshipRequest request)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.ReverseSort = false;
            request.AccountId = null;

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsRequest>(r => 
                   r.SearchFilters.EmployerName.Equals(request.EmployerName) &&
                   r.SearchFilters.CourseName.Equals(request.CourseName) &&
                   r.SearchFilters.Status.Equals(request.Status) &&
                   r.SearchFilters.StartDate.Equals(request.StartDate) &&
                   r.SearchFilters.EndDate.Equals(request.EndDate)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task GetApprenticesByPage([Frozen]GetApprenticeshipRequest request)
        {
            //Arrange
            request.ReverseSort = false;
            request.AccountId = null;

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsRequest>(r => 
                    r.ProviderId.Equals(request.ProviderId) &&
                    r.PageNumber.Equals(request.PageNumber) &&
                    r.PageItemCount.Equals(request.PageItemCount)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task GetFilterApprenticeshipsByPage([Frozen]GetApprenticeshipRequest request)
        {
            //Arrange
            request.ReverseSort = false;
            request.AccountId = null;

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsRequest>(r => 
                    r.SearchFilters.EmployerName.Equals(request.EmployerName) &&
                    r.SearchFilters.CourseName.Equals(request.CourseName) &&
                    r.SearchFilters.Status.Equals(request.Status) &&
                    r.SearchFilters.StartDate.Equals(request.StartDate) &&
                    r.SearchFilters.EndDate.Equals(request.EndDate)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnApprovedApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipRequest
            {
                ProviderId = 10
            };

            const int expectedTotalApprenticeshipsFound = 10;
            const int expectedTotalApprenticeshipsWithAlertsFound = 3;

            var expectedApprenticeship = new ApprenticeshipDetails
            {
                FirstName = "George",
                LastName = "Test",
                Uln = "12345",
                EmployerName = "Test Corp",
                CourseName = "Testing Level 1",
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddMonths(2),
                PaymentStatus = PaymentStatus.Active,
                Alerts = new []{"one", "two"}
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsRequest>(r => r.ProviderId.Equals(request.ProviderId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(new Application.Queries.GetApprenticeships.GetApprenticeshipsResponse
            {
                    Apprenticeships = new []{ expectedApprenticeship},
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

            Assert.AreEqual(expectedApprenticeship.FirstName, actualApprenticeship.FirstName);
            Assert.AreEqual(expectedApprenticeship.LastName, actualApprenticeship.LastName);
            Assert.AreEqual(expectedApprenticeship.Uln, actualApprenticeship.Uln);
            Assert.AreEqual(expectedApprenticeship.EmployerName, actualApprenticeship.EmployerName);
            Assert.AreEqual(expectedApprenticeship.CourseName, actualApprenticeship.CourseName);
            Assert.AreEqual(expectedApprenticeship.StartDate, actualApprenticeship.StartDate);
            Assert.AreEqual(expectedApprenticeship.EndDate, actualApprenticeship.EndDate);
            Assert.AreEqual(expectedApprenticeship.PaymentStatus, actualApprenticeship.PaymentStatus);
            Assert.AreEqual(expectedApprenticeship.Alerts, actualApprenticeship.Alerts);
            
            Assert.AreEqual(expectedTotalApprenticeshipsFound, response.TotalApprenticeshipsFound);
            Assert.AreEqual(expectedTotalApprenticeshipsWithAlertsFound, response.TotalApprenticeshipsWithAlertsFound);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprenticeships(new GetApprenticeshipRequest()) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }

    }
}
