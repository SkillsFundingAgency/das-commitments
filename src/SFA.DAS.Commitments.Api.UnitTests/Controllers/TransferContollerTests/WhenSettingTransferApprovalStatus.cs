using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.TransferContollerTests
{
    [TestFixture]
    public class WhenSettingTransferApprovalStatus
    {
        private Mock<IEmployerOrchestrator> _mockEmployerOrchestrator;
        private TransferController _sut;
        private CommitmentView _commitmentView = new CommitmentView();

        [SetUp]
        public void Setup()
        {
            _mockEmployerOrchestrator = new Mock<IEmployerOrchestrator>();
            _sut = new TransferController(_mockEmployerOrchestrator.Object);
        }

        [Test]
        public async Task ThenCallsOrchestratorWithTheCorrectParameters()
        {
            var request = new TransferApprovalRequest
            {
                TransferReceiverId = 112,
                TransferApprovalStatus = TransferApprovalStatus.Approved
            };

            await _sut.PatchTransferApprovalStatus(111, 3, 12, request);

            _mockEmployerOrchestrator.Verify(x => x.SetTransferApprovalStatus(111, 3, 12, request));
        }

    }
}
