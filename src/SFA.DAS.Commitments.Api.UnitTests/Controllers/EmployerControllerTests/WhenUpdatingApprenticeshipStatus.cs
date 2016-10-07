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
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Application.Exceptions;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenUpdatingApprenticeshipStatus
    {
        private const long TestProviderId = 1L;
        private const long TestCommitmentId = 2L;
        private const long TestApprenticeshipId = 3L;
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object);
            _controller = new EmployerController(_employerOrchestrator);
        }

        [Test]
        public async Task ThenANoContentCodeIsReturnedOnSuccess()
        {
            var result = await _controller.PatchApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, ApprenticeshipStatus.Approved);

            result.Should().BeOfType<StatusCodeResult>();

            (result as StatusCodeResult).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToUpdateApprenticeshipStatus()
        {
            var result = await _controller.PatchApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, ApprenticeshipStatus.Approved);

            _mockMediator.Verify(x => x.SendAsync(It.Is<UpdateApprenticeshipStatusCommand>(y => y.Status == ApprenticeshipStatus.Approved)));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<UpdateApprenticeshipStatusCommand>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.PatchApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, ApprenticeshipStatus.Approved));
        }
    }
}
