using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForReceiver;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingTransferRequestsForAReciever : EmployerOrchestratorTestBase
    {
        private List<TransferRequestSummary> _matches;

        [SetUp]
        public new void SetUp()
        {
            _matches = new List<TransferRequestSummary>();
            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetTransferRequestsForReceiverRequest>()))
                .ReturnsAsync(new GetTransferRequestsForReceiverResponse() { Data = _matches });

        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            //Act
            await Orchestrator.GetTransferRequestsForReceiver(123);
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<GetTransferRequestsForReceiverRequest>(p =>
                    p.TransferReceiverAccountId == 123)), Times.Once);
        }

        [Test]
        public async Task ThenShouldReturnFoundMatches()
        {
            // Arrange

            //Act
            var result = await Orchestrator.GetTransferRequestsForReceiver(123);

            Assert.AreSame(_matches, result);
        }
    }
}
