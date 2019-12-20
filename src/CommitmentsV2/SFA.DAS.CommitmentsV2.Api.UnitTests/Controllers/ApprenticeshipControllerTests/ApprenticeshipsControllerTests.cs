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
            var providerId = (uint) 10;

            //Act
            await _controller.GetApprenticeships(providerId);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsRequest>(r => r.ProviderId.Equals(providerId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnApprovedApprentices()
        {
            //Arrange
            var providerId = (uint)10;
            var expectedApprenticeship = new ApprenticeshipDetails
            {
                ApprenticeFirstName = "George",
                ApprenticeLastName = "Test",
                Uln = "12345",
                EmployerName = "Test Corp",
                CourseName = "Testing Level 1",
                PlannedStartDate = DateTime.Now.AddDays(2),
                PlannedEndDateTime = DateTime.Now.AddMonths(2),
                PaymentStatus = PaymentStatus.Active
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprenticeshipsRequest>(r => r.ProviderId.Equals(providerId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(new GetApprenticeshipsResponse
            {
                    Apprenticeships = new []{ expectedApprenticeship}
            });

            //Act
            var result = await _controller.GetApprenticeships(providerId) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);

            var apprentices = result.Value as ApprenticeshipDetails[];

            Assert.IsNotNull(apprentices);
            Assert.IsNotEmpty(apprentices);
            Assert.AreEqual(expectedApprenticeship, apprentices.First());
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
