using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public class WhenIGetAllProviderCommitments
    {
        private Mock<IMediator> _mockMediator;
        private ProviderController _controller;
        private ProviderOrchestrator _providerOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _providerOrchestrator = new ProviderOrchestrator(_mockMediator.Object);
            _controller = new ProviderController(_providerOrchestrator);
        }

        [Test, AutoData]
        public async Task ThenAListOfCommitmentsWillBeReturned(GetProviderCommitmentsResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetCommitments(1235L) as OkNegotiatedContentResult<IList<CommitmentListItem>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testProviderId = 1235L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).ReturnsAsync(new GetProviderCommitmentsResponse());

            var result = await _controller.GetCommitments(testProviderId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetProviderCommitmentsRequest>(arg => arg.ProviderId == testProviderId)));
        }

        [Test]
        public void ThenShouldReturnBadRequestIfThrowsAnInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetCommitments(1L));
        }
    }
}
