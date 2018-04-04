using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Commands.TransferApproval;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenSettingTransferApprovalStatus : EmployerOrchestratorTestBase
    {

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            //Arrange
            var fixture = new Fixture();

            var request = fixture.Build<TransferApprovalRequest>().Create();

            //Act
            await Orchestrator.SetTransferApprovalStatus(1, 2, 3, request);
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<TransferApprovalCommand>(p =>
                    p.TransferSenderId == 1 && p.CommitmentId == 2 && p.TransferRequestId == 3 &&
                    p.TransferApprovalStatus == (Domain.Entities.TransferApprovalStatus)request.TransferApprovalStatus && 
                    p.TransferReceiverId == request.TransferReceiverId)), Times.Once);
        }

    }
}
