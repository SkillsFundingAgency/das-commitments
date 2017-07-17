using NUnit.Framework;
using System.Threading.Tasks;
using MediatR;
using Moq;
using SFA.DAS.Commitments.Api.Controllers;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenGettingASingleCommitment
    {
        private Mock<IMediator> _mockMediator;
        private EmployerController _controller;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipOrchestor;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(), new FacetMapper(), new ApprenticeshipFilterService(new FacetMapper()), Mock.Of<IApprenticeshipMapper>());
            _apprenticeshipOrchestor = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipOrchestor);
        }

        [Test, AutoData]
        public async Task ThenReturnsASingleCommitment(GetCommitmentResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetCommitment(111L, 3L) as OkNegotiatedContentResult<CommitmentView>;

            result.Content.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
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
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentRequest>())).ReturnsAsync(new GetCommitmentResponse { Data = null });

            var result = await _controller.GetCommitment(111L, 0L);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
