using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;

using SFA.DAS.Commitments.Application.Queries.GetDataLocks;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Apprenticeship
{
    [TestFixture]
    public class WhenGettingDataLockSummary : ApprenticeshipOrchestratorTestBase
    {

        [SetUp]
        public void Arrange()
        {
            // Set up
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            var fixture = new Fixture();
            var failingDataLockFixture = fixture.Build<DataLockStatus>()
                .With(m => m.IsResolved, false)
                .With(m => m.Status, Status.Fail);

            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetDataLocksRequest>()))
                .ReturnsAsync(
                    new GetDataLocksResponse
                        {
                            Data = new List<DataLockStatus>
                                       {
                                           failingDataLockFixture.With(m => m.ErrorCode, DataLockErrorCode.Dlock04).Create(),
                                           failingDataLockFixture .With(m => m.ErrorCode, DataLockErrorCode.Dlock04 | DataLockErrorCode.Dlock07).Create(),
                                           failingDataLockFixture .With(m => m.ErrorCode, DataLockErrorCode.Dlock07).Create()
                                       }
                        });

            var result = await Orchestrator.GetDataLockSummary(1, new Caller(12345, CallerType.Provider));
            result.DataLockWithCourseMismatch.Count().Should().Be(2);
            result.DataLockWithOnlyPriceMismatch.Count().Should().Be(1);

            MockMediator.Verify(x => x.SendAsync(It.IsAny<GetDataLocksRequest>()), Times.Once);
        }
    }
}