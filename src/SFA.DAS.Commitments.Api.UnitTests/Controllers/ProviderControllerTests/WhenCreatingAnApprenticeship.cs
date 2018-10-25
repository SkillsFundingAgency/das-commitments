using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public class WhenCreatingAnApprenticeship
    {
        private const long TestProviderId = 1L;
        private const long TestCommitmentId = 2L;
        private ProviderController _controller;
        private Mock<IMediator> _mockMediator;
        private ProviderOrchestrator _providerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;
        protected Mock<FacetMapper> MockFacetMapper;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();

            MockFacetMapper = new Mock<FacetMapper>(Mock.Of<ICurrentDateTime>());

            _providerOrchestrator = new ProviderOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(),
                MockFacetMapper.Object,
                new ApprenticeshipFilterService(MockFacetMapper.Object),
                _apprenticeshipMapper.Object,
                Mock.Of<ICommitmentMapper>(),
                Mock.Of<IApprovedApprenticeshipMapper>());

            _apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentsLogger>());
            _controller = new ProviderController(_providerOrchestrator, _apprenticeshipsOrchestrator);
        }

        [Test]
        public async Task ThenACreateResponseCodeIsReturnedOnSuccess()
        {
            var result = await _controller.CreateApprenticeship(TestProviderId, TestCommitmentId, 
                new Apprenticeship.ApprenticeshipRequest { Apprenticeship = new Apprenticeship.Apprenticeship()});

            result.Should().BeOfType<CreatedAtRouteNegotiatedContentResult<Apprenticeship.Apprenticeship>>();
        }

        [Test]
        public async Task ThenTheLocationHeaderIsSetInTheResponseOnSuccessfulCreate()
        {
            const long createdApprenticeshipId = 10L;

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(createdApprenticeshipId);
            var result = await _controller.CreateApprenticeship(TestProviderId, TestCommitmentId,
                new Apprenticeship.ApprenticeshipRequest { Apprenticeship = new Apprenticeship.Apprenticeship() }) as CreatedAtRouteNegotiatedContentResult<Apprenticeship.Apprenticeship>;

            result.RouteName.Should().Be("GetApprenticeshipForProvider");
            result.RouteValues["providerId"].Should().Be(TestProviderId);
            result.RouteValues["commitmentId"].Should().Be(TestCommitmentId);
            result.RouteValues["apprenticeshipId"].Should().Be(createdApprenticeshipId);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateApprenticeship()
        {
            var newApprenticeship = new Apprenticeship.ApprenticeshipRequest
            {
                Apprenticeship = new Apprenticeship.Apprenticeship(),
                LastUpdatedByInfo = new LastUpdateInfo { EmailAddress = "test@email.com", Name = "Bob" }
            };
            var expectedApprenticeship = new Domain.Entities.Apprenticeship();

            _apprenticeshipMapper.Setup(m => m.Map(newApprenticeship.Apprenticeship, CallerType.Provider))
                .Returns(expectedApprenticeship);

            await _controller.CreateApprenticeship(TestProviderId, TestCommitmentId, newApprenticeship);
            _mockMediator.Verify(
                x =>
                    x.SendAsync(
                        It.Is<CreateApprenticeshipCommand>(
                            a =>
                                a.Caller.CallerType == CallerType.Provider && a.Caller.Id == TestProviderId && a.CommitmentId == TestCommitmentId &&
                                a.Apprenticeship == expectedApprenticeship  && a.UserName == newApprenticeship.LastUpdatedByInfo.Name)));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => 
                await _controller.CreateApprenticeship(TestProviderId, TestCommitmentId,
                    new Apprenticeship.ApprenticeshipRequest { Apprenticeship = new Apprenticeship.Apprenticeship() }));
        }
    }
}
