using System.Collections.Generic;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using SFA.DAS.Commitments.Application.Commands.TriageDataLocks;
using SFA.DAS.Commitments.Application.Queries.GetPriceHistory;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Apprenticeship
{
    [TestFixture]
    public class WhenGettingPriceHistory : ApprenticeshipOrchestratorTestBase
    {

        [SetUp]
        public void Arrange()
        {
            // Set up
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetPriceHistoryRequest>()))
                .ReturnsAsync(new GetPriceHistoryResponse { Data = new List<PriceHistory>() });
            await Orchestrator.GetPriceHistory(1, new Caller(12345, CallerType.Provider));

            MockMediator.Verify(x => x.SendAsync(It.IsAny<GetPriceHistoryRequest>()), Times.Once);
        }
    }
}