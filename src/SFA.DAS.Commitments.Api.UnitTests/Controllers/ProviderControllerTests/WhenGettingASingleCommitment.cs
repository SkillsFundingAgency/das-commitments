using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public class WhenGettingASingleCommitment
    {
        private Mock<IMediator> _mockMediator;
        private Mock<ICommitmentMapper> _commitmentMapper;
        private ProviderController _controller;
        private ProviderOrchestrator _providerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;
        private Mock<FacetMapper> _mockFacetMapper;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _commitmentMapper = new Mock<ICommitmentMapper>();
            _commitmentMapper.Setup(x => x.MapFrom(It.IsAny<Domain.Entities.Commitment>(), It.IsAny<CallerType>()))
                .Returns(new CommitmentView());

            _mockFacetMapper = new Mock<FacetMapper>(Mock.Of<ICurrentDateTime>());

            _providerOrchestrator = new ProviderOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(), 
                _mockFacetMapper.Object,
                new ApprenticeshipFilterService(_mockFacetMapper.Object),
                Mock.Of<IApprenticeshipMapper>(),
                _commitmentMapper.Object);

            _apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentsLogger>());
            _controller = new ProviderController(_providerOrchestrator, _apprenticeshipsOrchestrator);
        }

        [Test, AutoData]
        public async Task ThenReturnsASingleCommitment(GetCommitmentResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetCommitment(111L, 3L) as OkNegotiatedContentResult<CommitmentView>;

            result.Content.Should().NotBeNull();

            _commitmentMapper.Verify(x => x.MapFrom(It.IsAny<Domain.Entities.Commitment>(), It.IsAny<CallerType>()),
                Times.Once);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheCommitmentId()
        {
            const long testCommitmentId = 1235L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).ReturnsAsync(new GetCommitmentResponse());

            var result = await _controller.GetCommitment(111L, testCommitmentId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetCommitmentRequest>(arg => arg.CommitmentId == testCommitmentId)));
        }

        [TestCase]
        public void ThenReturnsABadResponseIfMediatorThrowsAInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetCommitment(111L, 0L));
        }

        [TestCase]
        public async Task ThenReturnsANotFoundIfMediatorReturnsANullForTheCommitement()
        {
            _commitmentMapper.Setup(x => x.MapFrom(It.IsAny<Domain.Entities.Commitment>(), It.IsAny<CallerType>()))
                .Returns(() => null);

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).ReturnsAsync(new GetCommitmentResponse { Data = null });

            var result = await _controller.GetCommitment(111L, 0L);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task ThenReturnsNotFoundWhenOrchestratorReturnsNull()
        {
            var orchestrator = new Mock<IProviderOrchestrator>();
            orchestrator.Setup(x => x.GetCommitment(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync((CommitmentView)null);
            var controller = new ProviderController(orchestrator.Object, Mock.Of<IApprenticeshipsOrchestrator>());

            var result = await controller.GetCommitment(1L, 1L);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
