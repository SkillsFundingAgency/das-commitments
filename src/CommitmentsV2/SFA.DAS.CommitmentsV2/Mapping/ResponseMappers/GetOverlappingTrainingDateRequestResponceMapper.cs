using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetOverlappingTrainingDateRequestResponceMapper : IMapper<GetOverlappingTrainingDateRequestQueryResult, GetOverlappingTrainingDateRequestResponce>
    {
        public Task<GetOverlappingTrainingDateRequestResponce> Map(GetOverlappingTrainingDateRequestQueryResult source)
        {
            return Task.FromResult(new GetOverlappingTrainingDateRequestResponce
            {
                OverlappingTrainingDateRequest = source.OverlappingTrainingDateRequests.Select(x => new ApprenticeshipOverlappingTrainingDateRequest
                {
                    Id = x.Id,
                    DraftApprenticeshipId = x.DraftApprenticeshipId,
                    PreviousApprenticeshipId = x.PreviousApprenticeshipId,
                    ResolutionType = x.ResolutionType,
                    Status = x.Status,
                    ActionedOn = x.ActionedOn
                }).ToList()
            });
        }
    }
}