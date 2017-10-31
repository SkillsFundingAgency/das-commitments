using System.Collections.Generic;
using System.Threading.Tasks;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Queries.GetDataLocks;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Apprenticeship
{
    [TestFixture]
    public class WhenGettingDataLocks : ApprenticeshipOrchestratorTestBase
    {

        [SetUp]
        public void Arrange()
        {
            // Set up
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetDataLocksRequest>()))
                .ReturnsAsync(new GetDataLocksResponse { Data = new List<DataLockStatus>() });
            await Orchestrator.GetDataLocks(1, new Caller(12345, CallerType.Provider));

            MockMediator.Verify(x => x.SendAsync(It.IsAny<GetDataLocksRequest>()), Times.Once);
        }
    }
}
