using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.HashingService;
using Commitment = SFA.DAS.Commitments.Api.Types.Commitment.Commitment;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenCreatingACommitment
    {
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;

        private Mock<ICommitmentMapper> _commitmentMapper;

        private Domain.Entities.Commitment _mappedCommitment;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _commitmentMapper = new Mock<ICommitmentMapper>();

            _mappedCommitment = new Fixture().Create<Domain.Entities.Commitment>();
            _commitmentMapper.Setup(m => m.MapFrom(It.IsAny<Commitment>()))
                .Returns(_mappedCommitment);

            _employerOrchestrator = new EmployerOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(), 
                new FacetMapper(Mock.Of<ICurrentDateTime>()), 
                new ApprenticeshipFilterService(new FacetMapper(Mock.Of<ICurrentDateTime>())), 
                Mock.Of<IApprenticeshipMapper>(), 
                _commitmentMapper.Object, Mock.Of<ITransferRequestMapper>(), Mock.Of<IHashingService>());

            var apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(
                _mockMediator.Object,
                Mock.Of<IDataLockMapper>(),
                Mock.Of<IApprenticeshipMapper>(),
                Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, apprenticeshipsOrchestrator);
        }

        [Test]
        public async Task ThenACreateResponseCodeIsReturnedOnSuccess()
        {
            var result = await _controller.CreateCommitment(123L, new CommitmentRequest { Commitment = new Commitment()});

            result.Should().BeOfType<CreatedAtRouteNegotiatedContentResult<CommitmentView>>();
        }

        [Test]
        public async Task ThenTheLocationHeaderIsSetInTheResponseOnSuccessfulCreate()
        {
            const long testAccountId = 123L;
            const long testCommitmentId = 5L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>())).ReturnsAsync(testCommitmentId);
            var result = await _controller.CreateCommitment(testAccountId,
                new CommitmentRequest { Commitment = new Commitment() }) as CreatedAtRouteNegotiatedContentResult<CommitmentView>;

            result.RouteName.Should().Be("GetCommitmentForEmployer");
            result.RouteValues["accountId"].Should().Be(testAccountId);
            result.RouteValues["commitmentId"].Should().Be(testCommitmentId);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateCommitment()
        {
            await _controller.CreateCommitment(123L, new CommitmentRequest { Commitment = new Commitment() });

            _mockMediator.Verify(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>()));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () =>
                await _controller.CreateCommitment(123L, new CommitmentRequest { Commitment = new Commitment() }));
        }
    }
}
