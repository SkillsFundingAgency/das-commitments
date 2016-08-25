using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Controllers;
using Moq;
using System.Web.Http.Results;
using SFA.DAS.Commitments.Domain;
using FluentAssertions;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using Ploeh.AutoFixture;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenIGetASingleCommitment
    {
        private Mock<IMediator> _mockMediator;
        private CommitmentsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new CommitmentsController(_mockMediator.Object);
        }

        [Test]
        public async Task ThenReturnsASingleCommitment()
        {
            var autoDataFixture = new Fixture();
            var mediatorResponse = autoDataFixture.Build<GetCommitmentResponse>().With(x => x.HasErrors, false).Create();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Returns(Task.FromResult(mediatorResponse));

            var result = await _controller.Get(2L) as OkNegotiatedContentResult<Commitment>;

            result.Content.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheCommitmentId()
        {
            const long testCommitmentId = 1235L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Returns(Task.FromResult(new GetCommitmentResponse()));

            var result = await _controller.Get(testCommitmentId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetCommitmentRequest>(arg => arg.CommitmentId == testCommitmentId)));
        }

        [TestCase]
        public async Task ThenReturnsABadResponseIfMediatorReturnsAnInvlidIdResult()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Returns(Task.FromResult(new GetCommitmentResponse { HasErrors = true }));

            var result = await _controller.Get(0L);

            result.Should().BeOfType<BadRequestResult>();
        }

        [TestCase]
        public async Task ThenReturnsANotFoundIfMediatorReturnsANullForTheCommitement()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).Returns(Task.FromResult(new GetCommitmentResponse { HasErrors = false, Data = null }));

            var result = await _controller.Get(0L);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
