using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlapRequests;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.OverlappingTrainingDateRequestControllerTests
{
    public class GetPendingOverlapRequestTests
    {
        [Test, MoqAutoData]
        public async Task WhenNoPendingOverlapRequests_ReturnEmptyResult(
            long draftApprenticeshipId,
            [Frozen] Mock<IMediator> mediatorMock,
            [Greedy] OverlappingTrainingDateRequestController _sut)
        {

            mediatorMock
                .Setup(x => x.Send(It.IsAny<GetPendingOverlapRequestsQuery>(), CancellationToken.None))
                .ReturnsAsync((GetPendingOverlapRequestsQueryResult)null);

            var result = await _sut.GetPendingOverlappingTrainingDateRequest(draftApprenticeshipId) as OkObjectResult;

            result.StatusCode.Should().Be(200);
            var response = result.Value as GetOverlapRequestsResponse;
            response.DraftApprenticeshipId.Should().BeNull();
        }

        [Test, MoqAutoData]
        public async Task WhenPendingOverlapRequestExists_ReturnResult(
            GetPendingOverlapRequestsQueryResult queryResult,
            [Frozen] Mock<IMediator> mediatorMock,
            [Greedy] OverlappingTrainingDateRequestController _sut)
        {
            mediatorMock
                .Setup(x => x.Send(It.IsAny<GetPendingOverlapRequestsQuery>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            var result = await _sut.GetPendingOverlappingTrainingDateRequest(queryResult.DraftApprenticeshipId.Value) as OkObjectResult;

            result.StatusCode.Should().Be(200);
            var response = result.Value as GetOverlapRequestsResponse;
            response.Should().BeEquivalentTo(queryResult);
        }
    }
}