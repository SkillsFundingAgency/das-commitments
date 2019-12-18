using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class ApprenticeshipToApprenticeshipDetailsMapper : IMapper<ApprovedApprenticeship, ApprenticeshipDetails>
    {
        public async Task<ApprenticeshipDetails> Map(ApprovedApprenticeship source)
        {
            await Task.CompletedTask;
            return new ApprenticeshipDetails
            {
                ApprenticeFirstName = source.FirstName,
                ApprenticeLastName = source.LastName,
                CourseName = source.CourseName,
                EmployerName = source.Cohort.LegalEntityName,
                PlannedStartDate = source.StartDate.GetValueOrDefault(),
                PlannedEndDateTime = source.EndDate.GetValueOrDefault(),
                PaymentStatus = source.PaymentStatus,
                Uln = source.Uln,
                Alerts = source.DataLockStatus.Select(status => status.Status.ToString())
            };
        }
    }
}