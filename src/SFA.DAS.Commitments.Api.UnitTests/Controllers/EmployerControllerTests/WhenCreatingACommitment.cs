using NUnit.Framework;
using Moq;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using FluentAssertions;
using System.Web.Http.Results;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Api.Controllers;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenCreatingACommitment
    {
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new EmployerController(_mockMediator.Object);
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
        public async Task ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>())).Throws<InvalidRequestException>();

            var result = await _controller.CreateCommitment(123L, new Commitment());

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}
