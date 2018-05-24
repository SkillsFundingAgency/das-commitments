using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForReceiver;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetTransferRequestsForReceiver
{
    [TestFixture]
    public class WhenGettingTransferRequestsForReceiver
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetTransferRequestsForReceiverQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetTransferRequestsForReceiverQueryHandler(_mockCommitmentRespository.Object);
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(new GetTransferRequestsForReceiverRequest
            {
                TransferReceiverAccountId = 1288
            });

            _mockCommitmentRespository.Verify(x => x.GetTransferRequestsForReceiver(1288), Times.Once);
        }
    }
}
