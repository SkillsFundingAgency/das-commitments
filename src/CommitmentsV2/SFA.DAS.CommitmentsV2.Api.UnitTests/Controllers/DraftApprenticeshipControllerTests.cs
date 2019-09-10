using System.Collections.Generic;
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
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Types.Dtos;
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

        [Test]
        public async Task Get_ValidRequest_ShouldReturnAnOkObjectResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithGetDraftApprenticeshipCommandResponse();

            //Act
            var response = await fixture.Get();

            //Assert
            Assert.IsTrue(response is OkObjectResult, $"Get method did not return a {nameof(OkObjectResult)} - returned a {response.GetType().Name} instead");
            var okObjectResult = (OkObjectResult) response;
            Assert.IsTrue(okObjectResult.Value is GetDraftApprenticeshipResponse, $"Get method did not return a value of type {nameof(GetDraftApprenticeshipResponse)} - returned a {okObjectResult.Value?.GetType().Name} instead");
        }

        [Test]
        public async Task GetAll_ValidRequest_ShouldReturnAnOkObjectResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithGetDraftApprenticeshipsRequestResponse();

            //Act
            var response = await fixture.GetAll();

            //Assert
            Assert.IsTrue(response is OkObjectResult, $"GetAll method did not return a {nameof(OkObjectResult)} - returned a {response.GetType().Name} instead");
            var okObjectResult = (OkObjectResult)response;
            Assert.IsTrue(okObjectResult.Value is IReadOnlyCollection<DraftApprenticeshipDto>, $"GetAll method did not return a value of type {nameof(GetDraftApprenticeshipsResponse)} - returned a {okObjectResult.Value?.GetType().Name} instead");
        }
    }

    public class DraftApprenticeshipControllerTestsFixture
    {
        public DraftApprenticeshipController Controller { get; set; }
        public Mock<IMediator> Mediator { get; set; }

        public UpdateDraftApprenticeshipRequest UpdateDraftApprenticeshipRequest { get; set; }
        public AddDraftApprenticeshipRequest AddDraftApprenticeshipRequest { get; set; }

        public UpdateDraftApprenticeshipCommand UpdateDraftApprenticeshipCommand { get; set; }
        public AddDraftApprenticeshipCommand AddDraftApprenticeshipCommand { get; set; }
        public GetDraftApprenticeRequest GetDraftApprenticeRequest { get; set; }
        public GetDraftApprenticeshipsRequest GetDraftApprenticeshipsRequest { get; set; }

        public Mock<IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>> UpdateDraftApprenticeshipMapper { get; set; }
        public Mock<IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>> AddDraftApprenticeshipMapper { get; set; }
        public Mock<IMapper<GetDraftApprenticeshipCommandResponse, GetDraftApprenticeshipResponse>> GetDraftApprenticeshipMapper { get; }
        public Mock<IMapper<GetDraftApprenticeshipsResult, GetDraftApprenticeshipsResponse>> GetDraftApprenticeshipsMapper { get; set; }

        public const long CohortId = 123;
        public const long DraftApprenticeshipId = 456;

        public DraftApprenticeshipControllerTestsFixture()
        {
            Mediator = new Mock<IMediator>();
            UpdateDraftApprenticeshipMapper = new Mock<IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>>();
            GetDraftApprenticeshipMapper = new Mock<IMapper<GetDraftApprenticeResponse, GetDraftApprenticeshipResponse>>();
            AddDraftApprenticeshipMapper = new Mock<IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>>();
            GetDraftApprenticeshipsMapper = new Mock<IMapper<GetDraftApprenticeshipsResult, GetDraftApprenticeshipsResponse>>();

            Controller = new DraftApprenticeshipController(
                Mediator.Object,
                UpdateDraftApprenticeshipMapper.Object,
                GetDraftApprenticeshipMapper.Object,
                AddDraftApprenticeshipMapper.Object,
                GetDraftApprenticeshipsMapper.Object
                );
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

        public DraftApprenticeshipControllerTestsFixture WithGetDraftApprenticeshipCommandResponse()
        {
            GetDraftApprenticeRequest = new GetDraftApprenticeRequest(CohortId, DraftApprenticeshipId);
            Mediator.Setup(m => m.Send(GetDraftApprenticeRequest, CancellationToken.None)).ReturnsAsync(new GetDraftApprenticeResponse{Id = DraftApprenticeshipId});
            GetDraftApprenticeshipMapper.Setup(m => m.Map(It.IsAny<GetDraftApprenticeshipCommandResponse>())).ReturnsAsync(new GetDraftApprenticeshipResponse());
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture WithGetDraftApprenticeshipsRequestResponse()
        {
            GetDraftApprenticeshipsRequest = new GetDraftApprenticeshipsRequest(CohortId);
            Mediator.Setup(m => m.Send(GetDraftApprenticeshipsRequest, CancellationToken.None)).ReturnsAsync(new GetDraftApprenticeshipsResult());
            GetDraftApprenticeshipsMapper.Setup(m => m.Map(It.IsAny<GetDraftApprenticeshipsResult>())).ReturnsAsync(new GetDraftApprenticeshipsResponse{ DraftApprenticeships = new List<DraftApprenticeshipDto>()});
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

        public Task<IActionResult> Get()
        {
            return Controller.Get(CohortId, DraftApprenticeshipId);
        }

        public Task<IActionResult> GetAll()
        {
            return Controller.GetAll(CohortId);
        }

    }
}