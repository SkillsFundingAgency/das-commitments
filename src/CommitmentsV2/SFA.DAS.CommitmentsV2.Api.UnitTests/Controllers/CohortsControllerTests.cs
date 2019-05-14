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
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class CohortsControllerTests
    {
        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnAnOkResult()
        {
            //Arrange
            var fixture = new CohortsControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();

            //Assert
            Assert.IsTrue(response is OkObjectResult);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedExpectedResponseObject()
        {
            //Arrange
            var fixture = new CohortsControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();

            //Assert
            Assert.IsTrue(((OkObjectResult)response).Value is CreateCohortResponse);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedCohortId()
        {
            //Arrange
            var fixture = new CohortsControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();
            var addCohortResponse = ((OkObjectResult)response).Value as CreateCohortResponse;

            //Assert
            Assert.AreEqual(CohortsControllerTestFixtures.CohortId, addCohortResponse.CohortId);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedReference()
        {
            //Arrange
            var fixture = new CohortsControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();
            var addCohortResponse = ((OkObjectResult)response).Value as CreateCohortResponse;

            //Assert
            Assert.AreEqual(CohortsControllerTestFixtures.CohortReference, addCohortResponse.CohortReference);
        }
    }

    public class CohortsControllerTestFixtures
    {
        public const long CohortId = 123;
        public const string CohortReference = "ABC123";
        public const long DraftApprenticeshipId = 456;

        public CohortsControllerTestFixtures()
        {
            MediatorMock = new Mock<IMediator>();
            CreateCohortRequestToAddCohortCommandMapperMock = new Mock<IMapper<CreateCohortRequest, AddCohortCommand>>();
        }

        private Mock<IMediator> MediatorMock { get; }
        private IMediator Mediator => MediatorMock.Object;
        private Mock<IMapper<CreateCohortRequest, AddCohortCommand>> CreateCohortRequestToAddCohortCommandMapperMock { get; }
        private IMapper<CreateCohortRequest, AddCohortCommand> CreateCohortRequestToAddCohortCommandMapper => CreateCohortRequestToAddCohortCommandMapperMock.Object;

        public CohortsController CreateController()
        {
            return new CohortsController(Mediator, CreateCohortRequestToAddCohortCommandMapper);
        }

        public CohortsControllerTestFixtures WithAddCohortCommandResponse(long id, string reference, long draftApprenticeshipId)
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

        public CohortsControllerTestFixtures WithAddCohortCommandResponse()
        {
            return WithAddCohortCommandResponse(CohortId, CohortReference, DraftApprenticeshipId);
        }

        public CohortsControllerTestFixtures WithAddDraftApprenticeshipCommandResponse(long id, string reference, long draftApprenticeshipId)
        {
            MediatorMock
                .Setup(m => m.Send(It.IsAny<AddDraftApprenticeshipCommand>(), CancellationToken.None))
                .ReturnsAsync(Unit.Value);

            return this;
        }

        public CohortsControllerTestFixtures WithAddDraftApprenticeshipCommandResponse()
        {
            return WithAddDraftApprenticeshipCommandResponse(CohortId, CohortReference, DraftApprenticeshipId);
        }

        public Task<IActionResult> CreateCohort()
        {
            var controller = CreateController();

            return controller.CreateCohort(new CreateCohortRequest());
        }
    }
}