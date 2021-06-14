using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetDataLockSummariesResponseMapper : IMapper<GetDataLockSummariesQueryResult, GetDataLockSummariesResponse>
    {
        public Task<GetDataLockSummariesResponse> Map(GetDataLockSummariesQueryResult result)
        {
            return Task.FromResult(new GetDataLockSummariesResponse
            {
                DataLocksWithCourseMismatch = result.DataLocksWithCourseMismatch,
                DataLocksWithOnlyPriceMismatch = result.DataLocksWithOnlyPriceMismatch
            });
        }
    }
}
