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
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;

namespace SFA.DAS.Commitments.Api.UnitTests.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenIGetAllProviderCommitments
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
        public async Task ThenAListOfCommitmentsWillBeReturned(GetProviderCommitmentsResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Returns(Task.FromResult(mediatorResponse));

            var result = await _controller.GetAll(1235L) as OkNegotiatedContentResult<IList<CommitmentListItem>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testProviderId = 1235L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Returns(Task.FromResult(new GetProviderCommitmentsResponse()));

            var result = await _controller.GetAll(testProviderId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetProviderCommitmentsRequest>(arg => arg.ProviderId == testProviderId)));
        }

        [Test]
        public async Task ThenShouldReturnBadRequestIfThrowsAnInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Throws<InvalidRequestException>();

            var result = await _controller.GetAll(1L);

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}
