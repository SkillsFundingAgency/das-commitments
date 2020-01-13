using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class ApprenticeshipToApprenticeshipDetailsMapper : IMapper<Apprenticeship, ApprenticeshipDetails>
    {
        private readonly IAlertsMapper _alertsMapper;

        public ApprenticeshipToApprenticeshipDetailsMapper(IAlertsMapper alertsMapper)
        {
            _alertsMapper = alertsMapper;
        }

        public Task<ApprenticeshipDetails> Map(Apprenticeship source)
        {
            return Task.FromResult(new ApprenticeshipDetails
            {
                ApprenticeshipId = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                CourseName = source.CourseName,
                EmployerName = source.Cohort.LegalEntityName,
                StartDate = source.StartDate.GetValueOrDefault(),
                EndDate = source.EndDate.GetValueOrDefault(),
                PaymentStatus = source.PaymentStatus,
                Uln = source.Uln,
                Alerts = _alertsMapper.Map(source)
            });
        }
    }
}