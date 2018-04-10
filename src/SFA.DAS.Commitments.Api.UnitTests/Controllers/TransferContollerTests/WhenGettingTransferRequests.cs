using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.TransferContollerTests
{
    [TestFixture]
    public class WhenGettingTransferRequests
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
            await _sut.GetTransferRequests("12");

            _mockEmployerOrchestrator.Verify(x => x.GetTransferRequests("12"));
        }

    }
}
