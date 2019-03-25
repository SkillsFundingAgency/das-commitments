using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class CohortControllerTests
    {
        [Test]
        public async Task CreateController_ValidRequest_ShouldReturnAnOkResult()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithCommandResponse();

            //Act
            var response = await fixture.RunAndReturnResponse();

            //Assert
            Assert.IsTrue(response is OkObjectResult);
        }

        [Test]
        public async Task CreateController_ValidRequest_ShouldReturnExpectedExpectedResponseObject()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithCommandResponse();

            //Act
            var response = await fixture.RunAndReturnResponse();

            //Assert
            Assert.IsTrue(((OkObjectResult)response).Value is CreateCohortResponse);
        }

        [Test]
        public async Task CreateController_ValidRequest_ShouldReturnExpectedCohortId()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithCommandResponse();

            //Act
            var response = await fixture.RunAndReturnResponse();
            var addCohortResponse = ((OkObjectResult)response).Value as CreateCohortResponse;

            //Assert
            Assert.AreEqual(CohortControllerTestFixtures.CohortId, addCohortResponse.CohortId);
        }

        [Test]
        public async Task CreateController_ValidRequest_ShouldReturnExpectedReference()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithCommandResponse();

            //Act
            var response = await fixture.RunAndReturnResponse();
            var addCohortResponse = ((OkObjectResult)response).Value as CreateCohortResponse;

            //Assert
            Assert.AreEqual(CohortControllerTestFixtures.Reference, addCohortResponse.CohortReference);
        }

    }

    public class CohortControllerTestFixtures
    {
        public const long CohortId = 123;
        public const string Reference = "ABC123";
        public const long DraftApprenticeshipId = 456;

        public CohortControllerTestFixtures()
        {
            MediatorMock = new Mock<IMediator>();
            MapperMock = new Mock<IMapper<CreateCohortRequest, AddCohortCommand>>();
        }

        private Mock<IMediator> MediatorMock { get; }
        private IMediator Mediator => MediatorMock.Object;

        private Mock<IMapper<CreateCohortRequest, AddCohortCommand>> MapperMock { get; }
        private IMapper<CreateCohortRequest, AddCohortCommand> Mapper => MapperMock.Object;

        public CohortController CreateController()
        {
            return new CohortController(Mediator, Mapper);
        }

        public CohortControllerTestFixtures WithCommandResponse(long id, string reference, long draftApprenticeshipId)
        {
            MediatorMock
                .Setup(m => m.Send(It.IsAny<AddCohortCommand>(), CancellationToken.None))
                .ReturnsAsync(new AddCohortResponse
                {
                    Id = id,
                    Reference = reference
                });

            return this;
        }

        public CohortControllerTestFixtures WithCommandResponse()
        {
            var fixtures = WithCommandResponse(CohortId, Reference, DraftApprenticeshipId);
            return this;
        }

        public Task<IActionResult> RunAndReturnResponse()
        {
            var controller = CreateController();

            return controller.CreateCohort(new CreateCohortRequest());
        }
    }
}