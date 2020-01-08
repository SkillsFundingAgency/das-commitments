using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class
        GetApprenticeshipUpdateMapper : IMapper<GetApprenticeshipUpdateQueryResult, GetApprenticeshipUpdateResponse>
    {
        public Task<GetApprenticeshipUpdateResponse> Map(GetApprenticeshipUpdateQueryResult source)
        {
            GetApprenticeshipUpdateResponse response = null;

            if (source != null)
            {
                response = new GetApprenticeshipUpdateResponse
                {
                    Id = source.Id,
                    ApprenticeshipId = source.ApprenticeshipId,
                    Party = source.Originator.ToParty(),
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    TrainingType = source.TrainingType,
                    TrainingCode = source.TrainingCode,
                    TrainingName = source.TrainingName,
                    Cost = source.Cost,
                    StartDate = source.StartDate,
                    EndDate = source.EndDate,
                    DateOfBirth = source.DateOfBirth
                };
            }

            return Task.FromResult(response);
        }
    }
}

