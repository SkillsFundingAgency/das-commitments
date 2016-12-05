using System;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenCreatingAnApprenticeship
    {
        private const long TestAccountId = 1L;
        private const long TestCommitmentId = 2L;
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>());
            _controller = new EmployerController(_employerOrchestrator);
        }

        [Test]
        public async Task ThenACreateResponseCodeIsReturnedOnSuccess()
        {
            var result = await _controller.CreateApprenticeship(TestAccountId, TestCommitmentId, new Apprenticeship());

            result.Should().BeOfType<CreatedAtRouteNegotiatedContentResult<Apprenticeship>>();
        }

        [Test]
        public async Task ThenTheLocationHeaderIsSetInTheResponseOnSuccessfulCreate()
        {
            const long createdApprenticeshipId = 10L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(createdApprenticeshipId);
            var result = await _controller.CreateApprenticeship(TestAccountId, TestCommitmentId, new Apprenticeship()) as CreatedAtRouteNegotiatedContentResult<Apprenticeship>;

            result.RouteName.Should().Be("GetApprenticeshipForEmployer");
            result.RouteValues["accountId"].Should().Be(TestAccountId);
            result.RouteValues["commitmentId"].Should().Be(TestCommitmentId);
            result.RouteValues["apprenticeshipId"].Should().Be(createdApprenticeshipId);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateApprenticeship()
        {
            var newApprenticeship = new Apprenticeship();
            var result = await _controller.CreateApprenticeship(TestAccountId, TestCommitmentId, newApprenticeship);

            _mockMediator.Verify(x => x.SendAsync(It.Is<CreateApprenticeshipCommand>(a => a.Caller.CallerType == CallerType.Employer && a.Caller.Id == TestAccountId && a.CommitmentId == TestCommitmentId && a.Apprenticeship == newApprenticeship)));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>())).ThrowsAsync(new ValidationException(""));

            Func<Task> act = async () => await _controller.CreateApprenticeship(TestAccountId, TestCommitmentId, new Apprenticeship());

            act.ShouldThrow<ValidationException>();

        }
    }
}
