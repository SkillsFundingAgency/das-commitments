using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForSender;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetTransferRequestsForSender
{
    [TestFixture]
    public class WhenGettingTransferRequestsForSender
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetTransferRequestsForSenderQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetTransferRequestsForSenderQueryHandler(_mockCommitmentRespository.Object);
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(new GetTransferRequestsForSenderRequest
            {
                TransferSenderAccountId = 1234
            });

            _mockCommitmentRespository.Verify(x => x.GetTransferRequestsForSender(1234), Times.Once);
        }
    }
}
