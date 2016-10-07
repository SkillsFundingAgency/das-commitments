using NUnit.Framework;
using System.Threading.Tasks;
using MediatR;
using Moq;
using SFA.DAS.Commitments.Api.Controllers;
using Ploeh.AutoFixture.NUnit3;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public sealed class WhenGettingASingleApprenticeship
    {
        private const long TestProviderId = 1;
        private const long TestCommitmentId = 2;
        private const long TestApprenticeshipId = 3L;
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
        public async Task ThenReturnsASingleApprenticeship(GetApprenticeshipResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId) as OkNegotiatedContentResult<Apprenticeship>;

            result.Content.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheCommitmentIdApprenticeshipIdProviderId()
        {
            const long testProviderId = 2222L;
            const long testCommitmentId = 1235L;
            const long testApprenticeshipId = 4321L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(new GetApprenticeshipResponse());

            var result = await _controller.GetApprenticeship(testProviderId, testCommitmentId, testApprenticeshipId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetApprenticeshipRequest>(arg => arg.CommitmentId == testCommitmentId && arg.ApprenticeshipId == testApprenticeshipId && arg.Caller.CallerType == CallerType.Provider && arg.Caller.Id == testProviderId)));
        }

        [TestCase]
        public void ThenReturnsABadResponseIfMediatorThrowsAInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId));
        }

        [TestCase]
        public async Task ThenReturnsANotFoundIfMediatorReturnsANullForTheCommitement()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipRequest>())).ReturnsAsync(new GetApprenticeshipResponse { Data = null });

            var result = await _controller.GetApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
