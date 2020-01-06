using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class
        GetApprenticeshipUpdateMapper : IMapper<GetApprenticeshipUpdateQueryResult, GetApprenticeshipUpdateResponse>
    {
        public Task<GetApprenticeshipUpdateResponse> Map(GetApprenticeshipUpdateQueryResult source)
        {
            GetApprenticeshipUpdateResponse.ApprenticeshipUpdate apprenticeshipUpdate = null;

            if (source?.PendingApprenticeshipUpdate != null)
            {
                var update = source.PendingApprenticeshipUpdate;

                apprenticeshipUpdate = new GetApprenticeshipUpdateResponse.ApprenticeshipUpdate
                {
                    Id = update.Id,
                    ApprenticeshipId = update.ApprenticeshipId,
                    Originator = update.Originator,
                    FirstName = update.FirstName,
                    LastName = update.LastName,
                    TrainingType = update.TrainingType,
                    TrainingCode = update.TrainingCode,
                    TrainingName = update.TrainingName,
                    Cost = update.Cost,
                    StartDate = update.StartDate,
                    EndDate = update.EndDate,
                    DateOfBirth = update.DateOfBirth
                };
            }

            return Task.FromResult(new GetApprenticeshipUpdateResponse
            {
                PendingApprenticeshipUpdate = apprenticeshipUpdate
            });
        }
    }
}

