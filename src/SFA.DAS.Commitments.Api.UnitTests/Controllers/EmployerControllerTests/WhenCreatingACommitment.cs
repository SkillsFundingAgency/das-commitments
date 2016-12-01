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
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenCreatingACommitment
    {
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ILog>());
            _controller = new EmployerController(_employerOrchestrator);
        }

        [Test]
        public async Task ThenACreateResponseCodeIsReturnedOnSuccess()
        {
            var result = await _controller.CreateCommitment(123L, new Commitment());

            result.Should().BeOfType<CreatedAtRouteNegotiatedContentResult<Commitment>>();
        }

        [Test]
        public async Task ThenTheLocationHeaderIsSetInTheResponseOnSuccessfulCreate()
        {
            const long testAccountId = 123L;
            const long testCommitmentId = 5L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>())).ReturnsAsync(testCommitmentId);
            var result = await _controller.CreateCommitment(testAccountId, new Commitment()) as CreatedAtRouteNegotiatedContentResult<Commitment>;

            result.RouteName.Should().Be("GetCommitmentForEmployer");
            result.RouteValues["accountId"].Should().Be(testAccountId);
            result.RouteValues["commitmentId"].Should().Be(testCommitmentId);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateCommitment()
        {
            var result = await _controller.CreateCommitment(123L, new Commitment());

            _mockMediator.Verify(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>()));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.CreateCommitment(123L, new Commitment()));
        }
    }
}
