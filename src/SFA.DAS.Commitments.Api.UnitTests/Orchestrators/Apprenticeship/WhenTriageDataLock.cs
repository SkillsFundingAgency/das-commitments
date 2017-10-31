using System.Threading.Tasks;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Application.Commands.TriageDataLock;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Apprenticeship
{
    [TestFixture]
    public class WhenTriageDataLock : ApprenticeshipOrchestratorTestBase
    {

        [SetUp]
        public void Arrange()
        {
            // Set up
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            await Orchestrator.TriageDataLock(1, 2, new DataLockTriageSubmission(), new Caller(12345, CallerType.Provider));

            MockMediator.Verify(x => x.SendAsync(It.IsAny<TriageDataLockCommand>()), Times.Once);
        }
    }
}