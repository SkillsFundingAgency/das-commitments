using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Mappers;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class ApprenticeshipToApprenticeshipDetailsMapper : IMapper<Apprenticeship, ApprenticeshipDetails>
    {
        public Task<ApprenticeshipDetails> Map(Apprenticeship source)
        {
            return Task.FromResult(new ApprenticeshipDetails
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                CourseName = source.CourseName,
                EmployerName = source.Cohort.LegalEntityName,
                StartDate = source.StartDate.GetValueOrDefault(),
                EndDate = source.EndDate.GetValueOrDefault(),
                PaymentStatus = source.PaymentStatus,
                ApprenticeshipStatus = source.ApprenticeshipStatus,
                Uln = source.Uln,
                Alerts = source.MapAlerts()
            });
        }
    }
}