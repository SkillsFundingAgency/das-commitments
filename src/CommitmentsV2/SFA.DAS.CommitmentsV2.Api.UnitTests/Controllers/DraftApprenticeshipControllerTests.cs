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
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Types.Dtos;
using GetDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetDraftApprenticeshipResponse;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningDetails;
using SFA.DAS.CommitmentsV2.Application.Commands.RecognisePriorLearning;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;

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
            Assert.That(response is OkResult);
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
            Assert.That(addDraftApprenticeshipResponse?.DraftApprenticeshipId, Is.EqualTo(DraftApprenticeshipControllerTestsFixture.DraftApprenticeshipId));
        }

        [Test]
        public async Task Get_ValidRequest_ShouldReturnAnOkObjectResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithGetDraftApprenticeshipCommandResponse();

            //Act
            var response = await fixture.Get();

            //Assert
            Assert.That(response is OkObjectResult, $"Get method did not return a {nameof(OkObjectResult)} - returned a {response.GetType().Name} instead");
            var okObjectResult = (OkObjectResult)response;
            Assert.That(okObjectResult.Value is GetDraftApprenticeshipResponse, $"Get method did not return a value of type {nameof(GetDraftApprenticeshipResponse)} - returned a {okObjectResult.Value?.GetType().Name} instead");
        }

        [Test]
        public async Task Get_InValidRequest_ShouldReturnNotFoundResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture();

            //Act
            var response = await fixture.Get();

            //Assert
            Assert.That(response is NotFoundResult, $"Get method did not return a {nameof(NotFoundResult)} - returned a {response.GetType().Name} instead");
        }

        [Test]
        public async Task GetAll_ValidRequest_ShouldReturnAnOkObjectResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithGetDraftApprenticeshipsRequestResponse();

            //Act
            var response = await fixture.GetAll();

            //Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response is OkObjectResult, $"GetAll method did not return a {nameof(OkObjectResult)} - returned a {response.GetType().Name} instead");
            var okObjectResult = (OkObjectResult)response;
            Assert.That(okObjectResult, Is.Not.Null);
            Assert.That(okObjectResult.Value is GetDraftApprenticeshipsResponse, $"GetAll method did not return a value of type {nameof(GetDraftApprenticeshipsResponse)} - returned a {okObjectResult.Value?.GetType().Name} instead");
        }

        [Test]
        public async Task Delete_ValidRequest_ShouldReturnAnOkResult()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithDeleteDraftApprenticeshipCommandResponse();

            //Act
            var response = await fixture.Delete();

            //Assert
            Assert.That(response is OkResult);
        }

        [Test]
        public async Task Delete_DeleteCommandHandler_CalledWith_CorrectParameter()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithDeleteDraftApprenticeshipCommandResponse();

            //Act
            await fixture.Delete();

            //Assert
            fixture.Verify_DeleteCommandHandler_CalledWith_CorrectParameter();
        }

        [Test]
        public async Task Set_RecognisePriorLearning_ShouldMapToCommandObjectAndReturnOkResponse()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithRecognisePriorLearningRequest();

            //Act
            await fixture.UpdateRecognisePriorLearning();

            //Assert
            fixture.VerifyRecognisePriorLearningCommandIsMappedCorrectly();
        }

        [Test]
        public async Task Set_PriorLearningDetails_ShouldMapToCommandObjectAndReturnOkResponse()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture().WithPriorLearningDetailsRequest();

            //Act
            await fixture.UpdatePriorLearningDetails();

            //Assert
            fixture.VerifyPriorLearningDetailsCommandIsMappedCorrectly();
        }

        [Test]
        public async Task Get_PriorLearningSummary_ShouldMapRouteParamsToCommandObject()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture();

            //Act
            await fixture.GetApprenticeshipPriorLearningSummary();

            //Assert
            fixture.VerifyGetPriorLearningSummaryIsMappedToQueryCorrectly();
        }

        [Test]
        public async Task Get_PriorLearningSummary_ShouldReturnCorrectResponse()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture()
                .WithGetDraftApprenticeshipPriorLearningSummaryQueryResponse();

            var expected = fixture.GetDraftApprenticeshipPriorLearningSummaryQueryResult;

            //Act
            var response = await fixture.GetApprenticeshipPriorLearningSummary();

            //Assert
            Assert.That(response is OkObjectResult);
            var okObjectResult = (OkObjectResult)response;
            Assert.That(okObjectResult, Is.Not.Null);
            var obj = okObjectResult.Value as GetDraftApprenticeshipPriorLearningSummaryResponse;
            Assert.That(obj, Is.Not.Null);

            Assert.That(obj.ApprenticeshipId, Is.EqualTo(DraftApprenticeshipControllerTestsFixture.DraftApprenticeshipId));
            Assert.That(obj.CohortId, Is.EqualTo(DraftApprenticeshipControllerTestsFixture.CohortId));
            Assert.That(obj.TrainingTotalHours, Is.EqualTo(expected.TrainingTotalHours));
            Assert.That(obj.DurationReducedByHours, Is.EqualTo(expected.DurationReducedByHours));
            Assert.That(obj.PriceReducedBy, Is.EqualTo(expected.PriceReducedBy));
            Assert.That(obj.FundingBandMaximum, Is.EqualTo(expected.FundingBandMaximum));
            Assert.That(obj.PercentageOfPriorLearning, Is.EqualTo(expected.PercentageOfPriorLearning));
            Assert.That(obj.MinimumPercentageReduction, Is.EqualTo(expected.MinimumPercentageReduction));
            Assert.That(obj.MinimumPriceReduction, Is.EqualTo(expected.MinimumPriceReduction));
            Assert.That(obj.RplPriceReductionError, Is.EqualTo(expected.RplPriceReductionError));
        }

        [Test]
        public async Task Get_PriorLearningSummary_ShouldReturnNotFoundIfNoRpl()
        {
            //Arrange
            var fixture = new DraftApprenticeshipControllerTestsFixture();

            //Act
            var response = await fixture.GetApprenticeshipPriorLearningSummary();

            //Assert
            Assert.That(response is NotFoundResult);
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
        public GetDraftApprenticeshipsQuery GetDraftApprenticeshipsQuery { get; set; }
        public DeleteDraftApprenticeshipRequest DeleteDraftApprenticeshipRequest { get; set; }
        public PriorLearningDetailsRequest PriorLearningDetailsRequest { get; set; }
        public RecognisePriorLearningRequest RecognisePriorLearningRequest { get; set; }
        public DeleteDraftApprenticeshipCommand DeleteDraftApprenticeshipCommand { get; set; }

        public Mock<IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>> UpdateDraftApprenticeshipMapper { get; set; }
        public Mock<IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>> AddDraftApprenticeshipMapper { get; set; }
        public Mock<IOldMapper<GetDraftApprenticeshipQueryResult, GetDraftApprenticeshipResponse>> GetDraftApprenticeshipMapper { get; }
        public Mock<IOldMapper<GetDraftApprenticeshipsQueryResult, GetDraftApprenticeshipsResponse>> GetDraftApprenticeshipsMapper { get; set; }
        public Mock<IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand>> DeleteDraftApprenticeshipMapper { get; set; }
        public GetDraftApprenticeshipPriorLearningSummaryQuery GetDraftApprenticeshipPriorLearningSummaryQuery { get; set; }
        public GetDraftApprenticeshipPriorLearningSummaryQueryResult GetDraftApprenticeshipPriorLearningSummaryQueryResult { get; set; }

        public const long CohortId = 123;
        public const long DraftApprenticeshipId = 456;

        public DraftApprenticeshipControllerTestsFixture()
        {
            Mediator = new Mock<IMediator>();
            UpdateDraftApprenticeshipMapper = new Mock<IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>>();
            GetDraftApprenticeshipMapper = new Mock<IOldMapper<GetDraftApprenticeshipQueryResult, GetDraftApprenticeshipResponse>>();
            AddDraftApprenticeshipMapper = new Mock<IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>>();
            GetDraftApprenticeshipsMapper = new Mock<IOldMapper<GetDraftApprenticeshipsQueryResult, GetDraftApprenticeshipsResponse>>();
            DeleteDraftApprenticeshipMapper = new Mock<IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand>>();

            GetDraftApprenticeshipPriorLearningSummaryQuery = new GetDraftApprenticeshipPriorLearningSummaryQuery(CohortId, DraftApprenticeshipId);
            GetDraftApprenticeshipPriorLearningSummaryQueryResult = new GetDraftApprenticeshipPriorLearningSummaryQueryResult
            {
                TrainingTotalHours = 1000,
                DurationReducedByHours = 100,
                PriceReducedBy = 1300,
                FundingBandMaximum = 20500,
                PercentageOfPriorLearning = 55,
                MinimumPercentageReduction = 4,
                MinimumPriceReduction = 95,
                RplPriceReductionError = true
            };

            Controller = new DraftApprenticeshipController(
                Mediator.Object,
                UpdateDraftApprenticeshipMapper.Object,
                GetDraftApprenticeshipMapper.Object,
                AddDraftApprenticeshipMapper.Object,
                GetDraftApprenticeshipsMapper.Object,
                DeleteDraftApprenticeshipMapper.Object
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

        public DraftApprenticeshipControllerTestsFixture WithGetDraftApprenticeshipPriorLearningSummaryQueryResponse()
        {
            Mediator.Setup(m => m.Send(It.IsAny<GetDraftApprenticeshipPriorLearningSummaryQuery>(),
                CancellationToken.None)).ReturnsAsync(GetDraftApprenticeshipPriorLearningSummaryQueryResult);

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

        public DraftApprenticeshipControllerTestsFixture WithRecognisePriorLearningRequest()
        {
            RecognisePriorLearningRequest = new RecognisePriorLearningRequest { RecognisePriorLearning = true};
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture WithPriorLearningDetailsRequest()
        {
            PriorLearningDetailsRequest = new PriorLearningDetailsRequest { DurationReducedBy = 8, PriceReducedBy = 989 };
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture VerifyRecognisePriorLearningCommandIsMappedCorrectly()
        {
            Mediator.Verify(x=>x.Send(It.Is<RecognisePriorLearningCommand>(p =>
            p.CohortId == CohortId && p.ApprenticeshipId == DraftApprenticeshipId &&
                p.RecognisePriorLearning == RecognisePriorLearningRequest.RecognisePriorLearning), It.IsAny<CancellationToken>()));
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture VerifyPriorLearningDetailsCommandIsMappedCorrectly()
        {
            Mediator.Verify(x => x.Send(It.Is<PriorLearningDetailsCommand>(p =>
                p.CohortId == CohortId && p.ApprenticeshipId == DraftApprenticeshipId &&
                p.DurationReducedBy == PriorLearningDetailsRequest.DurationReducedBy &&
                p.PriceReducedBy == PriorLearningDetailsRequest.PriceReducedBy), It.IsAny<CancellationToken>()));
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture VerifyGetPriorLearningSummaryIsMappedToQueryCorrectly()
        {
            Mediator.Verify(x => x.Send(It.Is<GetDraftApprenticeshipPriorLearningSummaryQuery>(p =>
                p.CohortId == CohortId && p.DraftApprenticeshipId == DraftApprenticeshipId), It.IsAny<CancellationToken>()));
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture WithGetDraftApprenticeshipCommandResponse()
        {
            Mediator.Setup(m => m.Send(It.Is<GetDraftApprenticeshipQuery>(x => x.CohortId == CohortId && x.DraftApprenticeshipId == DraftApprenticeshipId), CancellationToken.None)).ReturnsAsync(new GetDraftApprenticeshipQueryResult{Id = DraftApprenticeshipId});
            GetDraftApprenticeshipMapper.Setup(m => m.Map(It.IsAny<GetDraftApprenticeshipQueryResult>())).ReturnsAsync(new GetDraftApprenticeshipResponse());
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture WithGetDraftApprenticeshipsRequestResponse()
        {
            GetDraftApprenticeshipsQuery = new GetDraftApprenticeshipsQuery(CohortId);
            Mediator.Setup(m => m.Send(GetDraftApprenticeshipsQuery, CancellationToken.None)).ReturnsAsync(new GetDraftApprenticeshipsQueryResult());
            GetDraftApprenticeshipsMapper.Setup(m => m.Map(It.IsAny<GetDraftApprenticeshipsQueryResult>())).ReturnsAsync(new GetDraftApprenticeshipsResponse{ DraftApprenticeships = new List<DraftApprenticeshipDto>()});
            return this;
        }

        public DraftApprenticeshipControllerTestsFixture WithDeleteDraftApprenticeshipCommandResponse()
        {
            DeleteDraftApprenticeshipRequest = new DeleteDraftApprenticeshipRequest();
            DeleteDraftApprenticeshipCommand = new DeleteDraftApprenticeshipCommand();
            DeleteDraftApprenticeshipMapper.Setup(m => m.Map(DeleteDraftApprenticeshipRequest)).ReturnsAsync(DeleteDraftApprenticeshipCommand);
            return this;
        }

        public Task<IActionResult> Update()
        {
            return Controller.Update(CohortId, DraftApprenticeshipId, UpdateDraftApprenticeshipRequest);
        }

        public Task<IActionResult> UpdateRecognisePriorLearning()
        {
            return Controller.Update(CohortId, DraftApprenticeshipId, RecognisePriorLearningRequest);
        }

        public Task<IActionResult> UpdatePriorLearningDetails()
        {
            return Controller.Update(CohortId, DraftApprenticeshipId, PriorLearningDetailsRequest);
        }

        public Task<IActionResult> GetApprenticeshipPriorLearningSummary()
        {
            return Controller.GetApprenticeshipPriorLearningSummary(CohortId, DraftApprenticeshipId);
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

        public Task<IActionResult> Delete()
        {
            return Controller.Delete(CohortId, DraftApprenticeshipId, DeleteDraftApprenticeshipRequest);
        }

        public void Verify_DeleteCommandHandler_CalledWith_CorrectParameter()
        {
            Mediator.Verify(x =>
               x.Send(
               It.Is<DeleteDraftApprenticeshipCommand>(command =>
               command.ApprenticeshipId == DraftApprenticeshipId && command.CohortId == CohortId),
               It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}