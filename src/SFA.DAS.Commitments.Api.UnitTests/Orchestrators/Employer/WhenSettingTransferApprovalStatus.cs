using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Commands.ApproveTransferRequest;
using SFA.DAS.Commitments.Application.Commands.RejectTransferRequest;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenSettingTransferApprovalStatus : EmployerOrchestratorTestBase
    {

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediatorOnApproval()
        {
            //Arrange
            var fixture = new Fixture();

            var request = fixture.Build<TransferApprovalRequest>()
                .With(x => x.TransferApprovalStatus, TransferApprovalStatus.Approved)
                .Create();

            //Act
            await Orchestrator.SetTransferApprovalStatus(1, 2, 3, request);
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<ApproveTransferRequestCommand>(p =>
                    p.TransferSenderId == 1 && p.CommitmentId == 2 && p.TransferRequestId == 3 &&
                    p.TransferReceiverId == request.TransferReceiverId)), Times.Once);
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediatorOnRejection()
        {
            //Arrange
            var fixture = new Fixture();

            var request = fixture.Build<TransferApprovalRequest>()
                .With(x => x.TransferApprovalStatus, TransferApprovalStatus.Rejected)
                .Create();

            //Act
            await Orchestrator.SetTransferApprovalStatus(1, 2, 3, request);
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<RejectTransferRequestCommand>(p =>
                    p.TransferSenderId == 1 && p.CommitmentId == 2 && p.TransferRequestId == 3 &&
                    p.TransferReceiverId == request.TransferReceiverId)), Times.Once);
        }

    }
}
