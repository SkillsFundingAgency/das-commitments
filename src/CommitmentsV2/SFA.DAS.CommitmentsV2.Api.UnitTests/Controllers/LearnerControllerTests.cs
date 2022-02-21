using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable]
    public class LearnerControllerTests
    {
        private Mock<IMediator> _mediator;
        private LearnerController _controller;

        public LearnerControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new LearnerController(_mediator.Object);
        }

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task GetAllLearners_When_NoFilter_Then_ReturnValidResponse()
        {
            // Arrange

            _mediator.Setup(m => m.Send(It.IsAny<GetAllLearnersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAllLearnersQueryResult(
                    new List<Learner>() 
                    { 
                        new Learner()
                        {
                            ApprenticeshipId = 987468,
                            FirstName = "Test",
                            LastName = "Name8662400336",
                            ULN = "8662400336",
                            TrainingCode = "287",
                            TrainingCourseVersion = null,
                            TrainingCourseVersionConfirmed = false,
                            TrainingCourseOption = null,
                            StandardUId = null,
                            StartDate = DateTime.Parse("2020-11-01T00:00:00"),
                            EndDate = DateTime.Parse("2023-05-01T00:00:00"),
                            CreatedOn = DateTime.Parse("2020-11-12T13:21:26"),
                            UpdatedOn = null,
                            StopDate = null,
                            PauseDate = null,
                            CompletionDate = null,
                            UKPRN = 10002638,
                            LearnRefNumber = "RF8662400336",
                            PaymentStatus = 1,
                            EmployerName = "SFA",
                            EmployerAccountId = 1000
                        }
                    },
                    1,100, 1));

            // Act

            var result = await _controller.GetAllLearners();

            // Assert

            result.Should().NotBeNull();
            var jsonResult = result as OkObjectResult;
            var getAllLearnersResponse = jsonResult?.Value as GetAllLearnersResponse;
            getAllLearnersResponse.Learners.Should().HaveCount(1);
            getAllLearnersResponse.BatchNumber.Should().Be(1);
            getAllLearnersResponse.BatchSize.Should().Be(100);
            getAllLearnersResponse.TotalNumberOfBatches.Should().Be(1);
        }
    }
}
