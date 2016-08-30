using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;

namespace SFA.DAS.Commitments.Api.UnitTests.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenIGetAllEmployerCommitments
    {
        private Mock<IMediator> _mockMediator;
        private CommitmentsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new CommitmentsController(_mockMediator.Object);
        }

        [Test, AutoData]
        public async Task ThenAListOfCommitmentsWillBeReturned(GetEmployerCommitmentsResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).Returns(Task.FromResult(mediatorResponse));

            var result = await _controller.GetAll(1234L) as OkNegotiatedContentResult<IList<CommitmentListItem>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testAccountId = 1234L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).Returns(Task.FromResult(new GetEmployerCommitmentsResponse()));

            var result = await _controller.GetAll(testAccountId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetEmployerCommitmentsRequest>(arg => arg.AccountId == testAccountId)));
        }

        [Test]
        public async Task ThenShouldReturnBadRequestIfThrowsAnInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).Throws<InvalidRequestException>();

            var result = await _controller.GetAll(-1L);

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}
