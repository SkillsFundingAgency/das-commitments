using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest;
using SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.PauseApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;
using GetApprenticeshipsRequest = SFA.DAS.CommitmentsV2.Api.Types.Requests.GetApprenticeshipsRequest;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    public class ApprenticeshipControllerTests
    {
        private Mock<IMediator> _mediator;
        private Mock<ILogger<ApprenticeshipController>> _logger;
        private Mock<IModelMapper> _mapper;
        private ApprenticeshipController _controller;

        [SetUp]
        public void Init()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ApprenticeshipController>>();
            _mapper = new Mock<IModelMapper>();

            _controller = new ApprenticeshipController(_mediator.Object, _mapper.Object, _logger.Object);
        }

        [Test]
        public async Task GetProviderApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10
            };

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                    r.ProviderId.Equals(request.ProviderId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task UpdateEndDateOfCompletedRecord([Frozen] EditEndDateRequest request)
        {
            //Act
            await _controller.EditEndDate(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<EditEndDateRequestCommand>(r =>
                    r.ApprenticeshipId.Equals(request.ApprenticeshipId)
                    && r.EndDate == request.EndDate
                    && r.UserInfo == request.UserInfo),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetEmployerApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                AccountId = 10
            };

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                    r.EmployerAccountId.Equals(request.AccountId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task GetFilterApprenticeships([Frozen]GetApprenticeshipsRequest request)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.ReverseSort = false;
            request.AccountId = null;

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                   r.SearchFilters.SearchTerm.Equals(request.SearchTerm) &&
                   r.SearchFilters.EmployerName.Equals(request.EmployerName) &&
                   r.SearchFilters.CourseName.Equals(request.CourseName) &&
                   r.SearchFilters.Status.Equals(request.Status) &&
                   r.SearchFilters.StartDate.Equals(request.StartDate) &&
                   r.SearchFilters.EndDate.Equals(request.EndDate) &&
                   r.SearchFilters.AccountLegalEntityId.Equals(request.AccountLegalEntityId) &&
                   r.SearchFilters.StartDateRange.From.Equals(request.StartDateRangeFrom) &&
                   r.SearchFilters.StartDateRange.To.Equals(request.StartDateRangeTo)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task GetApprenticesByPage([Frozen]GetApprenticeshipsRequest request)
        {
            //Arrange
            request.ReverseSort = false;
            request.AccountId = null;

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                    r.ProviderId.Equals(request.ProviderId) &&
                    r.PageNumber.Equals(request.PageNumber) &&
                    r.PageItemCount.Equals(request.PageItemCount)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task GetFilterApprenticeshipsByPage([Frozen]GetApprenticeshipsRequest request)
        {
            //Arrange
            request.ReverseSort = false;
            request.AccountId = null;

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                    r.SearchFilters.EmployerName.Equals(request.EmployerName) &&
                    r.SearchFilters.CourseName.Equals(request.CourseName) &&
                    r.SearchFilters.Status.Equals(request.Status) &&
                    r.SearchFilters.StartDate.Equals(request.StartDate) &&
                    r.SearchFilters.EndDate.Equals(request.EndDate) &&
                    r.SearchFilters.AccountLegalEntityId.Equals(request.AccountLegalEntityId) &&
                    r.SearchFilters.StartDateRange.From.Equals(request.StartDateRangeFrom) &&
                    r.SearchFilters.StartDateRange.To.Equals(request.StartDateRangeTo)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ThenTheQueryResultIsMapped()
        {
            //Arrange
            var expectedProviderId = 10;
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = expectedProviderId
            };
            
            _mediator.Setup(x => x.Send(It.Is<GetApprenticeshipsQuery>(c => c.ProviderId.Value.Equals(expectedProviderId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApprenticeshipsQueryResult());

            //Act
            var result = await _controller.GetApprenticeships(request) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);
            _mapper.Verify(x=>x.Map<GetApprenticeshipsResponse>(It.IsAny<GetApprenticeshipsQueryResult>()), Times.Once);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprenticeships(new GetApprenticeshipsRequest()) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }


        [Test, MoqAutoData]
        public async Task StopApprenticeship(StopApprenticeshipRequest request, long apprenticeshipId)
        {
            //Arrange

            //Act
            await _controller.StopApprenticeship(apprenticeshipId, request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<StopApprenticeshipCommand>(c =>
                    c.AccountId == request.AccountId &&
                    c.ApprenticeshipId == apprenticeshipId &&
                    c.StopDate == request.StopDate &&
                    c.MadeRedundant == request.MadeRedundant && 
                    c.UserInfo == request.UserInfo),                   
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task WhenPostingPauseApprenticeship_ThenPauseCommandIsSent(PauseApprenticeshipRequest request)
        {
            _mediator.Setup(p => p.Send(It.IsAny<PauseApprenticeshipCommand>(), It.IsAny<CancellationToken>()));

            await _controller.Pause(request);

            _mediator.Verify(p => p.Send(It.Is<PauseApprenticeshipCommand>(c => c.ApprenticeshipId == request.ApprenticeshipId && c.UserInfo == request.UserInfo), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ResumeApprenticeship(ResumeApprenticeshipRequest request)
        {
            //Act
            await _controller.Resume(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<ResumeApprenticeshipCommand>(c =>
                    c.ApprenticeshipId == request.ApprenticeshipId &&
                    c.UserInfo == request.UserInfo),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task WhenPostingResumeApprenticeship_ThenResumeCommandIsSent(ResumeApprenticeshipRequest request)
        {
            _mediator.Setup(p => p.Send(It.IsAny<ResumeApprenticeshipCommand>(), It.IsAny<CancellationToken>()));

            await _controller.Resume(request);

            _mediator.Verify(p => p.Send(It.Is<ResumeApprenticeshipCommand>(c =>
                    c.ApprenticeshipId == request.ApprenticeshipId && c.UserInfo == request.UserInfo),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task WhenPostingUpdateApprenticeshipStopDate_ThenUpdateApprenticeshipStopDateCommandIsSent(long apprenticeshipId, ApprenticeshipStopDateRequest request)
        {
            //Arrange
            _mediator.Setup(p => p.Send(It.IsAny<UpdateApprenticeshipStopDateCommand>(), It.IsAny<CancellationToken>()));

            //Act
            await _controller.UpdateApprenticeshipStopDate(apprenticeshipId, request);

            //Assert
            _mediator.Verify(p => p.Send(It.Is<UpdateApprenticeshipStopDateCommand>(c =>
                    c.AccountId == request.AccountId &&
                    c.ApprenticeshipId == apprenticeshipId &&
                    c.StopDate == request.NewStopDate &&
                    c.UserInfo == request.UserInfo),
					It.IsAny<CancellationToken>()), Times.Once);
        }

		[Test, MoqAutoData]
        public async Task ValidateApprenticeshipForEdit(ValidateApprenticeshipForEditRequest request)
        {
            //Act
            await _controller.ValidateApprenticeshipForEdit(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.IsAny<ValidateApprenticeshipForEditCommand>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ValidateApprenticeshipForEditNotFound(ValidateApprenticeshipForEditRequest request)
        {
            _mapper.Setup(x => x.Map<ValidateApprenticeshipForEditCommand>(request)).ReturnsAsync(() =>new ValidateApprenticeshipForEditCommand());
            _mediator.Setup(p => p.Send(It.IsAny<ValidateApprenticeshipForEditCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);

            //Act
            var notFoundResult = await _controller.ValidateApprenticeshipForEdit(request) as NotFoundResult;

            //Assert
            Assert.IsNotNull(notFoundResult);
        }

        [Test, MoqAutoData]
        public async Task EditApprenticeshpCommandIsSent(EditApprenticeshipApiRequest request)
        {
            //Act
            await _controller.EditApprenticeship(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.IsAny<EditApprenticeshipCommand>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task EditApprenticeshpResponseIsReturned(EditApprenticeshipApiRequest request)
        {
            _mediator.Setup(p => p.Send(It.IsAny<EditApprenticeshipCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new EditApprenticeshipResponse { ApprenticeshipId = 1, NeedReapproval = true });

            //Act
           var result = await _controller.EditApprenticeship(request) as OkObjectResult;

            var response = result.WithModel<Types.Responses.EditApprenticeshipResponse>();

            //Assert
            Assert.AreEqual(true, response.NeedReapproval);
            Assert.AreEqual(1, response.ApprenticeshipId);
        }

        [Test, MoqAutoData]
        public async Task EditApprenticeshpNotFound(EditApprenticeshipApiRequest request)
        {
            _mediator.Setup(p => p.Send(It.IsAny<EditApprenticeshipCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);

            //Act
           var notFoundResult = await _controller.EditApprenticeship(request) as NotFoundResult; 

            //Assert
            Assert.IsNotNull(notFoundResult);
        }
    }
}
