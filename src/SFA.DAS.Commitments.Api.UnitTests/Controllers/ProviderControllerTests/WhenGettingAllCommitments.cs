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
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

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
            _providerOrchestrator = new ProviderOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(), 
                Mock.Of<FacetMapper>(),
                new ApprenticeshipFilterService(new FacetMapper()));
            _controller = new ProviderController(_providerOrchestrator);
        }

        [Test, AutoData]
        public async Task ThenAListOfCommitmentsWillBeReturned(GetCommitmentsResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetCommitments(1235L) as OkNegotiatedContentResult<IList<CommitmentListItem>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testProviderId = 1235L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ReturnsAsync(new GetCommitmentsResponse());

            var result = await _controller.GetCommitments(testProviderId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetCommitmentsRequest>(arg => arg.Caller.CallerType == CallerType.Provider && arg.Caller.Id == testProviderId)));
        }

        [Test]
        public void ThenShouldReturnBadRequestIfThrowsAnInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetCommitments(1L));
        }
    }
}
