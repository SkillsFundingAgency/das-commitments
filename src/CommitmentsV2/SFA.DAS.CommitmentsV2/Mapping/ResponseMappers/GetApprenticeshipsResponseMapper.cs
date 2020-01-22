using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using ApprenticeshipDetailsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse.ApprenticeshipDetailsResponse;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetApprenticeshipsResponseMapper : IMapper<GetApprenticeshipsQueryResult, GetApprenticeshipsResponse>
    {
        public Task<GetApprenticeshipsResponse> Map(GetApprenticeshipsQueryResult source)
        {
            return Task.FromResult(new GetApprenticeshipsResponse
            {
                TotalApprenticeshipsFound = source.TotalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = source.TotalApprenticeshipsWithAlertsFound,
                Apprenticeships = source.Apprenticeships.Select(MapApprenticeship),
                TotalApprenticeships = source.TotalApprenticeships
            });
        }

        private static ApprenticeshipDetailsResponse MapApprenticeship(ApprenticeshipDetails source)
        {
            return new ApprenticeshipDetailsResponse
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Uln = source.Uln,
                EmployerName = source.EmployerName,
                CourseName = source.CourseName,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                PaymentStatus = source.PaymentStatus,
                Alerts = source.Alerts
            };
        }
    }
}
