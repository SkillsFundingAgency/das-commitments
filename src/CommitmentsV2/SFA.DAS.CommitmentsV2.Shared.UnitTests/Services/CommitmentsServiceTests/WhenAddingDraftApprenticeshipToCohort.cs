using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services.CommitmentsServiceTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenAddingDraftApprenticeshipToCohort
    {
        private CommitmentsServiceTestFixtures _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CommitmentsServiceTestFixtures();
        }

        [Test]
        public async Task IfShouldPassRequestToApiCall()
        {
            await _fixture.Sut.AddDraftApprenticeshipToCohort(_fixture.CohortId, _fixture.AddDraftApprenticeshipRequest);

            _fixture.CommitmentsApiClientMock.Verify(x => x.AddDraftApprenticeship(_fixture.CohortId, _fixture.AddDraftApprenticeshipRequest, CancellationToken.None), Times.Once);
        }
    }
}