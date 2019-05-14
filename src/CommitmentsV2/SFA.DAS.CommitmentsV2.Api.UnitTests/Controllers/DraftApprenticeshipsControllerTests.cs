using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable]
    public class DraftApprenticeshipsControllerTests
    {
        [Test]
        public async Task AddDraftApprenticeship_ValidRequest_ShouldReturnAnOkResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipsControllerTestsFixture().WithAddDraftApprenticeshipCommandResponse();

            //Act
            var response = await fixture.AddDraftApprenticeship();

            //Assert
            Assert.IsTrue(response is OkObjectResult);
        }

        [Test]
        public async Task AddDraftApprenticeship_ValidRequest_ShouldReturnExpectedExpectedResponseObject()
        {
            //Arrange
            var fixture = new DraftApprenticeshipsControllerTestsFixture().WithAddDraftApprenticeshipCommandResponse();

            //Act
            var response = await fixture.AddDraftApprenticeship();

            //Assert
            Assert.IsTrue(((OkObjectResult)response).Value is AddDraftApprenticeshipResponse);
        }
    }

    public class DraftApprenticeshipsControllerTestsFixture
    {
        public const long CohortId = 123;
        public const string CohortReference = "ABC123";
        public const long DraftApprenticeshipId = 456;

        public DraftApprenticeshipsControllerTestsFixture()
        {
            MediatorMock = new Mock<IMediator>();
            AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapperMock = new Mock<IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>>();
        }

        private Mock<IMediator> MediatorMock { get; }
        private IMediator Mediator => MediatorMock.Object;
        private Mock<IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>> AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapperMock { get; }
        private IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper => AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapperMock.Object;

        public DraftApprenticeshipsController CreateController()
        {
            return new DraftApprenticeshipsController(Mediator, AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper);
        }

        public DraftApprenticeshipsControllerTestsFixture WithAddDraftApprenticeshipCommandResponse(long id, string reference, long draftApprenticeshipId)
        {
            MediatorMock
                .Setup(m => m.Send(It.IsAny<AddDraftApprenticeshipCommand>(), CancellationToken.None))
                .ReturnsAsync(Unit.Value);

            return this;
        }

        public DraftApprenticeshipsControllerTestsFixture WithAddDraftApprenticeshipCommandResponse()
        {
            return WithAddDraftApprenticeshipCommandResponse(CohortId, CohortReference, DraftApprenticeshipId);
        }

        public Task<IActionResult> AddDraftApprenticeship()
        {
            var controller = CreateController();

            return controller.AddDraftApprenticeship(new AddDraftApprenticeshipRequest());
        }
    }
}