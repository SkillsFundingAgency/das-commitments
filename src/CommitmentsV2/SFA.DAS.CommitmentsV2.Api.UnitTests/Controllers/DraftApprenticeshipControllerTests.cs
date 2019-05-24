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
    public class DraftApprenticeshipControllerTests
    {
        [Test]
        public async Task AddDraftApprenticeship_ValidRequest_ShouldReturnAnOkObjectResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithAddDraftApprenticeshipCommandResponse();

            //Act
            var response = await fixture.AddDraftApprenticeship();
            var okObjectResult = response as OkObjectResult;
            var addDraftApprenticeshipResponse = okObjectResult?.Value as AddDraftApprenticeshipResponse;
            
            //Assert
            Assert.AreEqual(DraftApprenticeshipControllerTestsFixture.DraftApprenticeshipId, addDraftApprenticeshipResponse?.DraftApprenticeshipId);
        }
    }

    public class DraftApprenticeshipControllerTestsFixture
    {
        public AddDraftApprenticeshipRequest AddDraftApprenticeshipRequest { get; set; }
        public DraftApprenticeshipController Controller { get; set; }
        public AddDraftApprenticeshipCommand AddDraftApprenticeshipCommand { get; set; }
        public Mock<IMediator> Mediator { get; set; }
        public Mock<IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>> AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper { get; set; }

        public const long CohortId = 123;
        public const long DraftApprenticeshipId = 456;

        public DraftApprenticeshipControllerTestsFixture()
        {
            Mediator = new Mock<IMediator>();
            AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper = new Mock<IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>>();
            Controller = new DraftApprenticeshipController(Mediator.Object, AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper.Object);
        }

        public DraftApprenticeshipControllerTestsFixture WithAddDraftApprenticeshipCommandResponse()
        {
            AddDraftApprenticeshipRequest = new AddDraftApprenticeshipRequest();
            AddDraftApprenticeshipCommand = new AddDraftApprenticeshipCommand();
            Mediator.Setup(m => m.Send(AddDraftApprenticeshipCommand, CancellationToken.None)).ReturnsAsync(new AddDraftApprenticeshipResult { Id = DraftApprenticeshipId });
            AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper.Setup(m => m.Map(AddDraftApprenticeshipRequest)).Returns(AddDraftApprenticeshipCommand);
            
            return this;
        }

        public Task<IActionResult> AddDraftApprenticeship()
        {
            return Controller.AddDraftApprenticeship(CohortId, AddDraftApprenticeshipRequest);
        }
    }
}