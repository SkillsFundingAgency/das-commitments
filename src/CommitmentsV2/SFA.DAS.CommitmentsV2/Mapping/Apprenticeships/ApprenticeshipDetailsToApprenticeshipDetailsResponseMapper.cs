using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using ApprenticeshipDetailsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse.ApprenticeshipDetailsResponse;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class ApprenticeshipDetailsToApprenticeshipDetailsResponseMapper : IMapper<ApprenticeshipDetails, ApprenticeshipDetailsResponse>
    {
        public Task<ApprenticeshipDetailsResponse> Map(ApprenticeshipDetails source)
        {
            return Task.FromResult(new ApprenticeshipDetailsResponse
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
                ApprenticeshipStatus = source.ApprenticeshipStatus,
                Alerts = source.Alerts
            });
        }
    }
}
