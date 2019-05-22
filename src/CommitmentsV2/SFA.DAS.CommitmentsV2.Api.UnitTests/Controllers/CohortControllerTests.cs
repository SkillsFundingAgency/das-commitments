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
    public class CohortControllerTests
    {
        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnAnOkResult()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();

            //Assert
            Assert.IsTrue(response is OkObjectResult);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedExpectedResponseObject()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();

            //Assert
            Assert.IsTrue(((OkObjectResult)response).Value is CreateCohortResponse);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedCohortId()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();
            var addCohortResponse = ((OkObjectResult)response).Value as CreateCohortResponse;

            //Assert
            Assert.AreEqual(CohortControllerTestFixtures.CohortId, addCohortResponse.CohortId);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedReference()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();
            var addCohortResponse = ((OkObjectResult)response).Value as CreateCohortResponse;

            //Assert
            Assert.AreEqual(CohortControllerTestFixtures.CohortReference, addCohortResponse.CohortReference);
        }
    }

    public class CohortControllerTestFixtures
    {
        public const long CohortId = 123;
        public const string CohortReference = "ABC123";
        public const long DraftApprenticeshipId = 456;

        public CohortControllerTestFixtures()
        {
            MediatorMock = new Mock<IMediator>();
            CreateCohortRequestToAddCohortCommandMapperMock = new Mock<IMapper<CreateCohortRequest, AddCohortCommand>>();
        }

        private Mock<IMediator> MediatorMock { get; }
        private IMediator Mediator => MediatorMock.Object;
        private Mock<IMapper<CreateCohortRequest, AddCohortCommand>> CreateCohortRequestToAddCohortCommandMapperMock { get; }
        private IMapper<CreateCohortRequest, AddCohortCommand> CreateCohortRequestToAddCohortCommandMapper => CreateCohortRequestToAddCohortCommandMapperMock.Object;

        public CohortController CreateController()
        {
            return new CohortController(Mediator, CreateCohortRequestToAddCohortCommandMapper);
        }

        public CohortControllerTestFixtures WithAddCohortCommandResponse(long id, string reference, long draftApprenticeshipId)
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

        public CohortControllerTestFixtures WithAddCohortCommandResponse()
        {
            return WithAddCohortCommandResponse(CohortId, CohortReference, DraftApprenticeshipId);
        }

        public CohortControllerTestFixtures WithAddDraftApprenticeshipCommandResponse(long id, string reference, long draftApprenticeshipId)
        {
            MediatorMock
                .Setup(m => m.Send(It.IsAny<AddDraftApprenticeshipCommand>(), CancellationToken.None))
                .ReturnsAsync(Unit.Value);

            return this;
        }

        public CohortControllerTestFixtures WithAddDraftApprenticeshipCommandResponse()
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