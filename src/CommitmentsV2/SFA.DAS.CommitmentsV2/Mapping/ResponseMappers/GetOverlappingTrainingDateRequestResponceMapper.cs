using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetOverlappingTrainingDateRequestResponceMapper : IMapper<GetOverlappingTrainingDateRequestQueryResult, GetOverlappingTrainingDateRequestResponce>
    {
        public Task<GetOverlappingTrainingDateRequestResponce> Map(GetOverlappingTrainingDateRequestQueryResult source)
        {
            return Task.FromResult(new GetOverlappingTrainingDateRequestResponce
            {
                Id = source.Id,
                DraftApprenticeshipId = source.DraftApprenticeshipId,
                PreviousApprenticeshipId = source.PreviousApprenticeshipId,
                ResolutionType = source.ResolutionType,
                Status = source.Status,
                EmployerAction = source.EmployerAction,
                ActionedOn = source.ActionedOn
            });
        }
    }
}