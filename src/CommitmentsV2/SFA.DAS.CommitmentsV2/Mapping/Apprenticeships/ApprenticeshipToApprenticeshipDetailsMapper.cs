using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class ApprenticeshipToApprenticeshipDetailsMapper : IMapper<Apprenticeship, ApprenticeshipDetails>
    {
        public Task<ApprenticeshipDetails> Map(Apprenticeship source)
        {
            return Task.FromResult(new ApprenticeshipDetails
            {
                ApprenticeFirstName = source.FirstName,
                ApprenticeLastName = source.LastName,
                CourseName = source.CourseName,
                EmployerName = source.Cohort.LegalEntityName,
                PlannedStartDate = source.StartDate.GetValueOrDefault(),
                PlannedEndDateTime = source.EndDate.GetValueOrDefault(),
                PaymentStatus = source.PaymentStatus,
                Uln = source.Uln,
                //Alerts = MapAlerts(source)
            });
        }
    }
}