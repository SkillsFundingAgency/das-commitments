using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public class WhenCreatingCommitment
    {
        private ProviderOrchestrator _orchestrator;
        private Mock<IMediator> _mediator;
        private Mock<ICommitmentMapper> _commitmentMapper;
        private Domain.Entities.Commitment _commitmentMapperResponse;
        private CommitmentRequest _validCommitmentRequest;
        private long _providerId = 123;

        [SetUp]
        public void Arrange()
        {
            _validCommitmentRequest = new CommitmentRequest
            {
                Commitment = new Commitment(),
                LastAction = LastAction.None,
                Message = "",
                UserId = "UserId"
            };

            _mediator = new Mock<IMediator>();
            _mediator.Setup(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>())).ReturnsAsync(789);
            _commitmentMapperResponse = new Domain.Entities.Commitment();

            _commitmentMapper = new Mock<ICommitmentMapper>();
            _commitmentMapper.Setup(x => x.MapFrom(It.IsAny<Commitment>())).Returns(_commitmentMapperResponse);

            var facetMapper = new FacetMapper(Mock.Of<ICurrentDateTime>());

            _orchestrator = new ProviderOrchestrator(_mediator.Object,
                Mock.Of<ICommitmentsLogger>(),
                facetMapper,
                new ApprenticeshipFilterService(facetMapper),
                Mock.Of<IApprenticeshipMapper>(),
                _commitmentMapper.Object
                );
        }

        [Test]
        public async Task ThenTheCommitmentIsMappedToADomainEntity()
        {
            await _orchestrator.CreateCommitment(_providerId, TestHelper.Clone(_validCommitmentRequest));
            _commitmentMapper.Verify(x => x.MapFrom(It.IsAny<Commitment>()), Times.Once);           
        }

        [Test]
        public async Task ThenTheMappedCommitmentIsCreated()
        {
            await _orchestrator.CreateCommitment(_providerId, TestHelper.Clone(_validCommitmentRequest));
            _mediator.Verify(x => x.SendAsync(It.Is<CreateCommitmentCommand>((c) => c.Commitment == _commitmentMapperResponse)), Times.Once);
        }

        [Test]
        public async Task ThenTheCommitmentIsMappedWithTheCorrectProviderId()
        {
            await _orchestrator.CreateCommitment(_providerId, TestHelper.Clone(_validCommitmentRequest));
            _commitmentMapper.Verify(x => x.MapFrom(It.Is<Commitment>((c)=> c.ProviderId == _providerId)), Times.Once);
        }

        [Test]
        public async Task ThenTheCommitmentIsCreatedWithTheCorrectCallerType()
        {
            await _orchestrator.CreateCommitment(_providerId, TestHelper.Clone(_validCommitmentRequest));
            _mediator.Verify(x => x.SendAsync(It.Is<CreateCommitmentCommand>((c) =>
                    c.Caller.Id == _providerId && c.Caller.CallerType == CallerType.Provider)),
                Times.Once);
        }

        [Test]
        public async Task ThenTheCommitmentIsCreatedWithTheCorrectUserId()
        {
            await _orchestrator.CreateCommitment(_providerId, TestHelper.Clone(_validCommitmentRequest));
            _mediator.Verify(x => x.SendAsync(It.Is<CreateCommitmentCommand>((c) => c.UserId == _validCommitmentRequest.UserId)), Times.Once);
        }

        [Test]
        public async Task ThenTheCommitmentIsCreatedWithTheCorrectMessage()
        {
            await _orchestrator.CreateCommitment(_providerId, TestHelper.Clone(_validCommitmentRequest));
            _mediator.Verify(x => x.SendAsync(It.Is<CreateCommitmentCommand>((c) => c.Message == _validCommitmentRequest.Message)), Times.Once);
        }

        [Test]
        public async Task ThenTheCommitmentIsCreatedWithTheCorrectLastAction()
        {
            await _orchestrator.CreateCommitment(_providerId, TestHelper.Clone(_validCommitmentRequest));
            _mediator.Verify(x => x.SendAsync(It.Is<CreateCommitmentCommand>((c) => (LastAction) c.LastAction == _validCommitmentRequest.LastAction)), Times.Once);
        }
    }
}
