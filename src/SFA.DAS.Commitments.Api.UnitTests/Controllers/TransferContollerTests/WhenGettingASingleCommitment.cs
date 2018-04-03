using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.TransferContollerTests
{
    [TestFixture]
    public class WhenGettingASingleCommitment
    {
        private Mock<IEmployerOrchestrator> _mockEmployerOrchestrator;
        private TransferController _sut;
        private CommitmentView _commitmentView = new CommitmentView();

        [SetUp]
        public void Setup()
        {
            _mockEmployerOrchestrator = new Mock<IEmployerOrchestrator>();
            _sut = new TransferController(_mockEmployerOrchestrator.Object);
            _mockEmployerOrchestrator.Setup(x => x.GetCommitment(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CallerType>())).ReturnsAsync(_commitmentView);
        }

        [Test]
        public async Task ThenReturnsASingleCommitment()
        {
            var result = await _sut.GetCommitment(111, 3) as OkNegotiatedContentResult<CommitmentView>;

            result.Content.Should().NotBeNull();
            result.Content.Should().Be(_commitmentView);
        }

        [Test]
        public async Task ThenCallsOrchestratorWithTheCorrectParameters()
        {
            await _sut.GetCommitment(111, 3);

            _mockEmployerOrchestrator.Verify(x => x.GetCommitment(111, 3, CallerType.TransferSender));
        }

        [Test]
        public async Task ThenReturnsANotFoundResponse()
        {
            _mockEmployerOrchestrator.Setup(x => x.GetCommitment(12, 3, It.IsAny<CallerType>())).ReturnsAsync(null);
            var result = await _sut.GetCommitment(12, 3);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
