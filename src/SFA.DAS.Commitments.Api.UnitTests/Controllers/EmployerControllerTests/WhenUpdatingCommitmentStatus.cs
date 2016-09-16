using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Application.Exceptions;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenUpdatingCommitmentStatus
    {
        private const long TestProviderId = 1L;
        private const long TestCommitmentId = 2L;
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new EmployerController(_mockMediator.Object);
        }

        [Test]
        public async Task ThenANoContentCodeIsReturnedOnSuccess()
        {
            var result = await _controller.PatchCommitment(TestProviderId, TestCommitmentId, new CommitmentStatusChange { Status = CommitmentStatus.Active});

            result.Should().BeOfType<StatusCodeResult>();

            (result as StatusCodeResult).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateCommitment()
        {
            var result = await _controller.PatchCommitment(TestProviderId, TestCommitmentId, new CommitmentStatusChange { Status = CommitmentStatus.Active });

            _mockMediator.Verify(x => x.SendAsync(It.Is<UpdateCommitmentStatusCommand>(y => y.Status == CommitmentStatus.Active)));
        }

        [Test]
        public async Task ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<UpdateCommitmentStatusCommand>())).Throws<InvalidRequestException>();

            var result = await _controller.PatchCommitment(TestProviderId, TestCommitmentId, new CommitmentStatusChange { Status = CommitmentStatus.Active });

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}
