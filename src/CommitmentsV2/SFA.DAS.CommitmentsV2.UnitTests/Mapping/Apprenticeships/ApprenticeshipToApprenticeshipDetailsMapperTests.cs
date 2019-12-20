using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Apprenticeships
{
    public class ApprenticeshipToApprenticeshipDetailsMapperTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Maps_Apprenticeship_To_ApprenticeshipDetails(
            Apprenticeship source,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            var result = await mapper.Map(source);

            result.ApprenticeFirstName.Should().Be(source.FirstName);
            result.ApprenticeLastName.Should().Be(source.LastName);
            result.CourseName.Should().Be(source.CourseName);
            result.EmployerName.Should().Be(source.Cohort.LegalEntityName);
            result.PlannedStartDate.Should().Be(source.StartDate.Value);
            result.PlannedEndDateTime.Should().Be(source.EndDate.Value);
            result.PaymentStatus.Should().Be(source.PaymentStatus);
            result.Uln.Should().Be(source.Uln);
            result.Alerts.Should().BeEquivalentTo(source.DataLockStatus.Select(status => status.Status.ToString()));
        }
    }
}