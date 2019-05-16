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
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping;
using UpdateDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.UpdateDraftApprenticeshipResponse;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class DraftApprenticeshipControllerTests
    {
        [Test]
        public async Task Update_ValidRequest_ShouldReturnAnOkResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestFixtures().WithCommandResponse();

            //Act
            var response = await fixture.Update(1,1, new UpdateDraftApprenticeshipRequest());

            //Assert
            Assert.IsTrue(response is OkObjectResult);
        }

        [Test]
        public async Task Update_ValidRequest_ShouldReturnExpectedExpectedResponseObject()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestFixtures().WithCommandResponse();

            //Act
            var response = await fixture.Update(1, 1, new UpdateDraftApprenticeshipRequest());

            //Assert
            Assert.IsTrue(((OkObjectResult)response).Value is UpdateDraftApprenticeshipResponse);
        }
     }

    public class DraftApprenticeshipControllerTestFixtures
    {
        public const long CohortId = 123;
        public const string Reference = "ABC123";
        public const long DraftApprenticeshipId = 456;

        public DraftApprenticeshipControllerTestFixtures()
        {
            MediatorMock = new Mock<IMediator>();
            MapperMock = new Mock<IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>>();

            MapperMock
                .Setup(m => m.Map(It.IsAny<UpdateDraftApprenticeshipRequest>()))
                .Returns((UpdateDraftApprenticeshipRequest request) => new UpdateDraftApprenticeshipCommand());
        }

        private Mock<IMediator> MediatorMock { get; }
        private IMediator Mediator => MediatorMock.Object;

        private Mock<IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>> MapperMock { get; }
        private IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> Mapper => MapperMock.Object;

        public DraftApprenticeshipController CreateController()
        {
            return new DraftApprenticeshipController(Mediator, Mapper);
        }

        public DraftApprenticeshipControllerTestFixtures WithCommandResponse(long id, string reference, long draftApprenticeshipId)
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

        public DraftApprenticeshipControllerTestFixtures WithCommandResponse()
        {
            WithCommandResponse(CohortId, Reference, DraftApprenticeshipId);
            return this;
        }

        public Task<IActionResult> Update(long cohortId, long apprenticeshipId, UpdateDraftApprenticeshipRequest request)
        {
            var controller = CreateController();

            return controller.Update(cohortId, apprenticeshipId, request);
        }
    }
}