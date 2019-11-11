using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services.CommitmentsServiceTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenUpdatingDraftApprenticeship
    {
        private CommitmentsServiceTestFixtures _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CommitmentsServiceTestFixtures();
        }

        [Test]
        public async Task IfShouldForwardRequestToApiCall()
        {
            var updateRequest = new UpdateDraftApprenticeshipRequest();
            await _fixture.Sut.UpdateDraftApprenticeship(1, 2, updateRequest);

            _fixture.CommitmentsApiClientMock.Verify(x => x.UpdateDraftApprenticeship(1, 2, updateRequest, CancellationToken.None), Times.Once);
        }
    }
}