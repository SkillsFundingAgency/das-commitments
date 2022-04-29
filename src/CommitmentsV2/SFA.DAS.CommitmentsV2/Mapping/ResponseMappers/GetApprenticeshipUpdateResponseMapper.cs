using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetApprenticeshipUpdateResponseMapper : IMapper<GetApprenticeshipUpdateQueryResult, GetApprenticeshipUpdatesResponse>
    {
        public Task<GetApprenticeshipUpdatesResponse> Map(GetApprenticeshipUpdateQueryResult sources)
        {
            return Task.FromResult(new GetApprenticeshipUpdatesResponse
            {
                ApprenticeshipUpdates = sources.ApprenticeshipUpdates.Select(source => new GetApprenticeshipUpdatesResponse.ApprenticeshipUpdate
                {
                    Id = source.Id,
                    ApprenticeshipId = source.ApprenticeshipId,
                    OriginatingParty = source.Originator.ToParty(),
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    Email = source.Email,
                    DeliveryModel = source.DeliveryModel,
                    EmploymentEndDate = source.EmploymentEndDate,
                    EmploymentPrice = source.EmploymentPrice,
                    TrainingType = source.TrainingType,
                    TrainingCode = source.TrainingCode,
                    Version = source.TrainingCourseVersion,
                    TrainingName = source.TrainingName,
                    Option = source.TrainingCourseOption,
                    Cost = source.Cost,
                    StartDate = source.StartDate,
                    EndDate = source.EndDate,
                    DateOfBirth = source.DateOfBirth
                }).ToList()
            });
        }
    }
}