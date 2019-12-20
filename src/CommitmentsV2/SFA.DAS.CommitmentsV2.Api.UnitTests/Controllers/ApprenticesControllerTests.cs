using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    public class ApprenticesControllerTests
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
            await _controller.GetApprovedApprentices(providerId);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprovedApprenticesRequest>(r => r.ProviderId.Equals(providerId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ReturnApprovedApprentices()
        {
            //Arrange
            var providerId = (uint)10;
            var expectedApprenticeship = new ApprenticeshipDetails
            {
                ApprenticeName = "Mr Test",
                Uln = "12345",
                EmployerName = "Test Corp",
                CourseName = "Testing Level 1",
                PlannedStartDate = DateTime.Now.AddDays(2),
                PlannedEndDateTime = DateTime.Now.AddMonths(2),
                Status = "Planned"
            };

            _mediator.Setup(m => m.Send(It.Is<GetApprovedApprenticesRequest>(r => r.ProviderId.Equals(providerId)),
                It.IsAny<CancellationToken>())).ReturnsAsync(new GetApprovedApprenticesResponse
            {
                    Apprenticeships = new []{ expectedApprenticeship}
            });

            //Act
            var result = await _controller.GetApprovedApprentices(providerId) as OkObjectResult;

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
            var result = await _controller.GetApprovedApprentices(10) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }

    }
}
