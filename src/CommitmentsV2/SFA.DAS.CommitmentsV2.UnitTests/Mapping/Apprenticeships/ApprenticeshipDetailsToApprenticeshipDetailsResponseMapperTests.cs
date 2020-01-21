using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Apprenticeships
{
    public class ApprenticeshipDetailsToApprenticeshipDetailsResponseMapperTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Maps_ApprenticeshipDetailsToApprenticeshipDetailsResponse(
            ApprenticeshipDetails source,
            ApprenticeshipDetailsToApprenticeshipDetailsResponseMapper mapper)
        {
            var result = await mapper.Map(source);

            result.Id.Should().Be(source.Id);
            result.FirstName.Should().Be(source.FirstName);
            result.LastName.Should().Be(source.LastName);
            result.CourseName.Should().Be(source.CourseName);
            result.EmployerName.Should().Be(source.EmployerName);
            result.StartDate.Should().Be(source.StartDate);
            result.EndDate.Should().Be(source.EndDate);
            result.PaymentStatus.Should().Be(source.PaymentStatus);
            result.Uln.Should().Be(source.Uln);
            result.Alerts.Should().BeEquivalentTo(source.Alerts);
        }
    }
}
