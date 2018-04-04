using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForSender;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingTransferRequestsForASender : EmployerOrchestratorTestBase
    {
        private List<TransferRequestSummary> _matches;

        [SetUp]
        public new void SetUp()
        {
            _matches = new List<TransferRequestSummary>();
            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetTransferRequestsForSenderRequest>()))
                .ReturnsAsync(new GetTransferRequestsForSenderResponse { Data = _matches });

        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            //Act
            await Orchestrator.GetTransferRequestsForSender(123);
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<GetTransferRequestsForSenderRequest>(p =>
                    p.TransferSenderAccountId == 123)), Times.Once);
        }

        [Test]
        public async Task ThenShouldReturnFoundMatches()
        {
            // Arrange

            //Act
            var result = await Orchestrator.GetTransferRequestsForSender(123);

            Assert.AreSame(_matches, result);
        }
    }
}
