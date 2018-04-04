using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenUpdatingApprenticeshipStatus
    {
        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();

            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(),
                new FacetMapper(Mock.Of<ICurrentDateTime>()),
                new ApprenticeshipFilterService(new FacetMapper(Mock.Of<ICurrentDateTime>())),
                Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentMapper>(), Mock.Of<ITransferRequestMapper>());

            _apprenticeshipOrchestor = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(),
                Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipOrchestor);
        }

        private const long TestProviderId = 1L;
        private const long TestApprenticeshipId = 3L;
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipOrchestor;


        [TestCase(PaymentStatus.Deleted)]
        [TestCase(PaymentStatus.Completed)]
        [TestCase(PaymentStatus.PendingApproval)]
        public void ThenWhenRequestingInvalidChangeThrowsException(PaymentStatus invalidStatus)
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await
                    _controller.PatchApprenticeship(TestProviderId, TestApprenticeshipId,
                        new ApprenticeshipSubmission {PaymentStatus = invalidStatus}))
                .Message.Should().Contain("Not a valid value for change of status");
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Withdrawn)]
        public void ThenWhenRequestingAValidChangeDoesNotThrowException(PaymentStatus validStatus)
        {
            Assert.DoesNotThrowAsync(async () => await
                _controller.PatchApprenticeship(TestProviderId, TestApprenticeshipId,
                    new ApprenticeshipSubmission {PaymentStatus = validStatus}));
        }


        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<ResumeApprenticeshipCommand>()))
                .ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await
                _controller.PatchApprenticeship(TestProviderId, TestApprenticeshipId,
                    new ApprenticeshipSubmission {PaymentStatus = PaymentStatus.Active}));
        }

        [Test]
        public async Task ThenANoContentCodeIsReturnedOnSuccess()
        {
            var result = await _controller.PatchApprenticeship(
                TestProviderId,
                TestApprenticeshipId,
                new ApprenticeshipSubmission {PaymentStatus = PaymentStatus.Active});

            result.Should().BeOfType<StatusCodeResult>();

            (result as StatusCodeResult).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToPauseAnApprenticeship()
        {
            var userName = "Bob";
            await _controller.PatchApprenticeship(TestProviderId, TestApprenticeshipId,
                new ApprenticeshipSubmission
                {
                    PaymentStatus = PaymentStatus.Paused,
                    LastUpdatedByInfo = new LastUpdateInfo {Name = userName}
                });

            _mockMediator.Verify(x => x.SendAsync(It.Is<PauseApprenticeshipCommand>(y => y.UserName == userName)));
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToResumeAnApprenticeship()
        {
            var userName = "Bob";
            await _controller.PatchApprenticeship(TestProviderId, TestApprenticeshipId,
                new ApprenticeshipSubmission
                {
                    PaymentStatus = PaymentStatus.Active,
                    LastUpdatedByInfo = new LastUpdateInfo {Name = userName}
                });

            _mockMediator.Verify(x => x.SendAsync(It.Is<ResumeApprenticeshipCommand>(y => y.UserName == userName)));
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToStopAnApprenticeship()
        {
            var userName = "Bob";
            await _controller.PatchApprenticeship(TestProviderId, TestApprenticeshipId,
                new ApprenticeshipSubmission
                {
                    PaymentStatus = PaymentStatus.Withdrawn,
                    LastUpdatedByInfo = new LastUpdateInfo {Name = userName}
                });

            _mockMediator.Verify(x => x.SendAsync(It.Is<StopApprenticeshipCommand>(y => y.UserName == userName)));
        }
    }
}