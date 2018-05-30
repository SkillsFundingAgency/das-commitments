using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.TransferContollerTests
{
    [TestFixture]
    public class WhenGettingTransferRequestForSender
    {
        private Mock<IEmployerOrchestrator> _mockEmployerOrchestrator;
        private TransferController _sut;

        [SetUp]
        public void Setup()
        {
            _mockEmployerOrchestrator = new Mock<IEmployerOrchestrator>();
            _sut = new TransferController(_mockEmployerOrchestrator.Object);
        }

        [Test]
        public async Task ThenCallsOrchestratorWithTheCorrectParameters()
        {
            await _sut.GetTransferRequestForSender(111, 222);

            _mockEmployerOrchestrator.Verify(x => x.GetTransferRequest(222, 111, CallerType.TransferSender));
        }

    }
}
