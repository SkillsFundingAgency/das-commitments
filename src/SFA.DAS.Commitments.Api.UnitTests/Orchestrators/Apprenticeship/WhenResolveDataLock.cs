using System.Threading.Tasks;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using SFA.DAS.Commitments.Application.Commands.ApproveDataLockTriage;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Apprenticeship
{
    [TestFixture]
    public class WhenResolveDataLock : ApprenticeshipOrchestratorTestBase
    {

        [SetUp]
        public void Arrange()
        {
            // Set up
        }

        [Test]
        public async Task ShouldResolveDataLock()
        {
            var subm = new DataLocksTriageResolutionSubmission();
            subm.DataLockUpdateType = DataLockUpdateType.ApproveChanges;

            await Orchestrator.ResolveDataLock(1, subm, new Caller(12345, CallerType.Provider));

            MockMediator.Verify(x => x.SendAsync(It.IsAny<ApproveDataLockTriageCommand>()), Times.Once);
        }

        [Test]
        public async Task ShoulRejectDataLock()
        {
            var subm = new DataLocksTriageResolutionSubmission();
            subm.DataLockUpdateType = DataLockUpdateType.ApproveChanges;

            await Orchestrator.ResolveDataLock(1, subm, new Caller(12345, CallerType.Provider));

            MockMediator.Verify(x => x.SendAsync(It.IsAny<ApproveDataLockTriageCommand>()), Times.Once);
        }
    }
}