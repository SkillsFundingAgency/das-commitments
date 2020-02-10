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

            result.Id.Should().Be(source.Id);
            result.FirstName.Should().Be(source.FirstName);
            result.LastName.Should().Be(source.LastName);
            result.CourseName.Should().Be(source.CourseName);
            result.EmployerName.Should().Be(source.Cohort.AccountLegalEntity.Name);
            result.StartDate.Should().Be(source.StartDate.Value);
            result.EndDate.Should().Be(source.EndDate.Value);
            result.PaymentStatus.Should().Be(source.PaymentStatus);
            result.Uln.Should().Be(source.Uln);
        }

    }
}