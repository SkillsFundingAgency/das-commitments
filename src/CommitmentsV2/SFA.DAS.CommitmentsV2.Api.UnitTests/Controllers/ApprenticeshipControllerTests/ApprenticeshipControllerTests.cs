﻿using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.PauseApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.ResendInvitation;
using SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateUln;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;
using GetApprenticeshipsRequest = SFA.DAS.CommitmentsV2.Api.Types.Requests.GetApprenticeshipsRequest;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    public class ApprenticeshipControllerTests
    {
        private Mock<IMediator> _mediator;
        private Mock<ILogger<ApprenticeshipController>> _logger;
        private Mock<IModelMapper> _mapper;
        private Mock<IAuthenticationService> _authService;
        private ApprenticeshipController _controller;

        [SetUp]
        public void Init()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ApprenticeshipController>>();
            _mapper = new Mock<IModelMapper>();
            _authService = new Mock<IAuthenticationService>();

            _authService.Setup(x => x.GetUserParty()).Returns(Party.Employer);

            _controller = new ApprenticeshipController(_mediator.Object, _mapper.Object, _authService.Object, _logger.Object);
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
        public async Task GetFilterApprenticeships([Frozen] GetApprenticeshipsRequest request)
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
                   r.SearchFilters.StartDateRange.To.Equals(request.StartDateRangeTo) &&
                   r.SearchFilters.Alert == request.Alert &&
                   r.SearchFilters.IsOnFlexiPaymentPilot == request.IsOnFlexiPaymentPilot),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task GetApprenticesByPage([Frozen] GetApprenticeshipsRequest request)
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
        public async Task GetFilterApprenticeshipsByPage([Frozen] GetApprenticeshipsRequest request)
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
                    r.SearchFilters.StartDateRange.To.Equals(request.StartDateRangeTo) &&
                    r.SearchFilters.IsOnFlexiPaymentPilot == request.IsOnFlexiPaymentPilot),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ThenTheQueryResultIsMapped()
        {
            //Arrange
            const int expectedProviderId = 10;
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
            Assert.That(result, Is.Not.Null);
            _mapper.Verify(x=>x.Map<GetApprenticeshipsResponse>(It.IsAny<GetApprenticeshipsQueryResult>()), Times.Once);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprenticeships(new GetApprenticeshipsRequest()) as NotFoundResult;

            //Assert
            Assert.That(result, Is.Not.Null);
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
                    c.UserInfo == request.UserInfo &&
                    c.Party == Party.Employer),
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
        public async Task WhenPostingResendInviation_ThenResendInvitation(long apprenticeshipId, SaveDataRequest request)
        {
            //Act
            await _controller.ResendInvitation(apprenticeshipId, request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<ResendInvitationCommand>(c =>
                    c.ApprenticeshipId == apprenticeshipId &&
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
            _mapper.Setup(x => x.Map<ValidateApprenticeshipForEditCommand>(request)).ReturnsAsync(() => new ValidateApprenticeshipForEditCommand());
            _mediator.Setup(p => p.Send(It.IsAny<ValidateApprenticeshipForEditCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);

            //Act
            var notFoundResult = await _controller.ValidateApprenticeshipForEdit(request) as NotFoundResult;

            //Assert
            Assert.That(notFoundResult, Is.Not.Null);
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
        public async Task EditApprenticeshipResponseIsReturned(EditApprenticeshipApiRequest request)
        {
            _mediator.Setup(p => p.Send(It.IsAny<EditApprenticeshipCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new EditApprenticeshipResponse { ApprenticeshipId = 1, NeedReapproval = true });

            //Act
            var result = await _controller.EditApprenticeship(request) as OkObjectResult;

            var response = result.WithModel<Types.Responses.EditApprenticeshipResponse>();

            Assert.Multiple(() =>
            {
                //Assert
                Assert.That(response.NeedReapproval, Is.EqualTo(true));
                Assert.That(response.ApprenticeshipId, Is.EqualTo(1));
            });
        }

        [Test, MoqAutoData]
        public async Task EditApprenticeshipNotFound(EditApprenticeshipApiRequest request)
        {
            _mediator.Setup(p => p.Send(It.IsAny<EditApprenticeshipCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);

            //Act
           var notFoundResult = await _controller.EditApprenticeship(request) as NotFoundResult;

            //Assert
            Assert.That(notFoundResult, Is.Not.Null);
        }

        [Test, MoqAutoData]
        public async Task ValidateUlnOverlap(ValidateUlnOverlapRequest request)
        {
            //Act
            await _controller.ValidateUlnOverlap(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.IsAny<ValidateUlnOverlapCommand>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ValidateUlnOverlapNotFound(ValidateUlnOverlapRequest request)
        {
            _mediator.Setup(p => p.Send(It.IsAny<ValidateUlnOverlapCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => null);

            //Act
            var notFoundResult = await _controller.ValidateUlnOverlap(request) as NotFoundResult;

            //Assert
            Assert.That(notFoundResult, Is.Not.Null);
        }
    }
}
