using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class ApprenticeshipToApprenticeshipDetailsMapper : IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>
    {
        private readonly ICurrentDateTime _currentDateTime;

        public ApprenticeshipToApprenticeshipDetailsMapper(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }
        public Task<GetApprenticeshipsQueryResult.ApprenticeshipDetails> Map(Apprenticeship source)
        {
            return Task.FromResult(new GetApprenticeshipsQueryResult.ApprenticeshipDetails
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                CourseName = source.CourseName,
                EmployerName = source.Cohort.LegalEntityName,
                StartDate = source.StartDate.GetValueOrDefault(),
                EndDate = source.EndDate.GetValueOrDefault(),
                PaymentStatus = source.PaymentStatus,
                ApprenticeshipStatus = source.MapApprenticeshipStatus(_currentDateTime),
                Uln = source.Uln,
                Alerts = source.MapAlerts()
            });
        }
    }
}