using NUnit.Framework;
using System.Threading.Tasks;
using MediatR;
using Moq;
using SFA.DAS.Commitments.Api.Controllers;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using System.Web.Http.Results;
using SFA.DAS.Commitments.Api.Types;
using FluentAssertions;
using SFA.DAS.Commitments.Application.Exceptions;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenGettingASingleCommitment
    {
        private Mock<IMediator> _mockMediator;
        private EmployerController _controller;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new EmployerController(_mockMediator.Object);
        }

        [Test, AutoData]
        public async Task ThenReturnsASingleCommitment(GetCommitmentResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Returns(Task.FromResult(mediatorResponse));

            var result = await _controller.GetCommitment(111L, 3L) as OkNegotiatedContentResult<Commitment>;

            result.Content.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheCommitmentId()
        {
            const long testCommitmentId = 1235L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Returns(Task.FromResult(new GetCommitmentResponse()));

            var result = await _controller.GetCommitment(111L, testCommitmentId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetCommitmentRequest>(arg => arg.CommitmentId == testCommitmentId)));
        }

        [TestCase]
        public async Task ThenReturnsABadResponseIfMediatorThrowsAInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Throws<InvalidRequestException>();

            var result = await _controller.GetCommitment(111L, 0L);

            result.Should().BeOfType<BadRequestResult>();
        }

        [TestCase]
        public async Task ThenReturnsAUnauthorizedResponseIfMediatorThrowsAnNotAuthorizedException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Throws<UnauthorizedException>();

            var result = await _controller.GetCommitment(111L, 0L);

            result.Should().BeOfType<UnauthorizedResult>();
        }

        [TestCase]
        public async Task ThenReturnsANotFoundIfMediatorReturnsANullForTheCommitement()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Returns(Task.FromResult(new GetCommitmentResponse { Data = null }));

            var result = await _controller.GetCommitment(111L, 0L);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
