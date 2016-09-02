using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public class WhenCreatingAnApprenticeship
    {
        private const long TestProviderId = 1L;
        private const long TestCommitmentId = 2L;
        private ProviderController _controller;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new ProviderController(_mockMediator.Object);
        }

        [Test]
        public async Task ThenACreateResponseCodeIsReturnedOnSuccess()
        {
            var result = await _controller.CreateApprenticeship(TestProviderId, TestCommitmentId, new Apprenticeship());

            result.Should().BeOfType<CreatedAtRouteNegotiatedContentResult<Apprenticeship>>();
        }

        [Test]
        public async Task ThenTheLocationHeaderIsSetInTheResponseOnSuccessfulCreate()
        {
            const long createdApprenticeshipId = 10L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(createdApprenticeshipId);
            var result = await _controller.CreateApprenticeship(TestProviderId, TestCommitmentId, new Apprenticeship()) as CreatedAtRouteNegotiatedContentResult<Apprenticeship>;

            result.RouteName.Should().Be("GetApprenticeshipForProvider");
            result.RouteValues["providerId"].Should().Be(TestProviderId);
            result.RouteValues["commitmentId"].Should().Be(TestCommitmentId);
            result.RouteValues["apprenticeshipId"].Should().Be(createdApprenticeshipId);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateApprenticeship()
        {
            var result = await _controller.CreateApprenticeship(TestProviderId, TestCommitmentId, new Apprenticeship());

            _mockMediator.Verify(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>()));
        }

        [Test]
        public async Task ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>())).Throws<InvalidRequestException>();

            var result = await _controller.CreateApprenticeship(TestProviderId, TestCommitmentId, new Apprenticeship());

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}
