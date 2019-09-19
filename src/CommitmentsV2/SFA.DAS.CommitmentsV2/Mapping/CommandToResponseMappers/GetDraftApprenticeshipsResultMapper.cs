using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;

namespace SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers
{
    public class GetDraftApprenticeshipsResultMapper : IMapper<GetDraftApprenticeshipsResult, GetDraftApprenticeshipsResponse>
    {
        public Task<GetDraftApprenticeshipsResponse> Map(GetDraftApprenticeshipsResult source)
        {
            return Task.FromResult(new GetDraftApprenticeshipsResponse
            {
                DraftApprenticeships = source.DraftApprenticeships
            });
        }
    }
}
