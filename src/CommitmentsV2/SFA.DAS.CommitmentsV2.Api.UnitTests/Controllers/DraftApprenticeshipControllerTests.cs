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
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice;
using SFA.DAS.CommitmentsV2.Mapping;

using GetDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetDraftApprenticeshipResponse;
using GetDraftApprenticeshipCommandResponse = SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice.GetDraftApprenticeResponse;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable]
    public class DraftApprenticeshipControllerTests
    {
        [Test]
        public async Task Update_ValidRequest_ShouldReturnAnOkResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithUpdateDraftApprenticeshipCommandResponse();

            //Act
            var response = await fixture.Update();

            //Assert
            Assert.IsTrue(response is OkResult);
        }
        
        [Test]
        public async Task Add_ValidRequest_ShouldReturnAnOkObjectResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithAddDraftApprenticeshipCommandResponse();

            //Act
            var response = await fixture.Add();
            var okObjectResult = response as OkObjectResult;
            var addDraftApprenticeshipResponse = okObjectResult?.Value as AddDraftApprenticeshipResponse;
            
            //Assert
            Assert.AreEqual(DraftApprenticeshipControllerTestsFixture.DraftApprenticeshipId, addDraftApprenticeshipResponse?.DraftApprenticeshipId);
        }
    }

    public class DraftApprenticeshipControllerTestsFixture
    {
        public UpdateDraftApprenticeshipRequest UpdateDraftApprenticeshipRequest { get; set; }
        public AddDraftApprenticeshipRequest AddDraftApprenticeshipRequest { get; set; }
        public DraftApprenticeshipController Controller { get; set; }
        public UpdateDraftApprenticeshipCommand UpdateDraftApprenticeshipCommand { get; set; }
        public AddDraftApprenticeshipCommand AddDraftApprenticeshipCommand { get; set; }
        public Mock<IMediator> Mediator { get; set; }
        public Mock<IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>> UpdateDraftApprenticeshipMapper { get; set; }
        public Mock<IMapper<GetDraftApprenticeshipCommandResponse, GetDraftApprenticeshipResponse>> GetDraftApprenticeshipMapper { get; }
        public Mock<IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>> AddDraftApprenticeshipMapper { get; set; }

        public const long CohortId = 123;
        public const long DraftApprenticeshipId = 456;

        public DraftApprenticeshipControllerTestsFixture()
        {
            Mediator = new Mock<IMediator>();
            UpdateDraftApprenticeshipMapper = new Mock<IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>>();
            GetDraftApprenticeshipMapper = new Mock<IMapper<GetDraftApprenticeResponse, GetDraftApprenticeshipResponse>>();
            AddDraftApprenticeshipMapper = new Mock<IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>>();
            
            Controller = new DraftApprenticeshipController(
                Mediator.Object,
                UpdateDraftApprenticeshipMapper.Object,
                GetDraftApprenticeshipMapper.Object,
                AddDraftApprenticeshipMapper.Object);
        }

        public DraftApprenticeshipControllerTestsFixture WithUpdateDraftApprenticeshipCommandResponse()
        {
            UpdateDraftApprenticeshipRequest = new UpdateDraftApprenticeshipRequest();
            UpdateDraftApprenticeshipCommand = new UpdateDraftApprenticeshipCommand();
            UpdateDraftApprenticeshipMapper.Setup(m => m.Map(UpdateDraftApprenticeshipRequest)).ReturnsAsync(UpdateDraftApprenticeshipCommand);
            Mediator.Setup(m => m.Send(UpdateDraftApprenticeshipCommand, CancellationToken.None)).ReturnsAsync(new UpdateDraftApprenticeshipResponse { Id = CohortId, ApprenticeshipId = DraftApprenticeshipId });
            
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture WithAddDraftApprenticeshipCommandResponse()
        {
            AddDraftApprenticeshipRequest = new AddDraftApprenticeshipRequest();
            AddDraftApprenticeshipCommand = new AddDraftApprenticeshipCommand();
            AddDraftApprenticeshipMapper.Setup(m => m.Map(AddDraftApprenticeshipRequest)).ReturnsAsync(AddDraftApprenticeshipCommand);
            Mediator.Setup(m => m.Send(AddDraftApprenticeshipCommand, CancellationToken.None)).ReturnsAsync(new AddDraftApprenticeshipResult { Id = DraftApprenticeshipId });
            
            return this;
        }

        public Task<IActionResult> Update()
        {
            return Controller.Update(CohortId, DraftApprenticeshipId, UpdateDraftApprenticeshipRequest);
        }

        public Task<IActionResult> Add()
        {
            return Controller.Add(CohortId, AddDraftApprenticeshipRequest);
        }
    }
}