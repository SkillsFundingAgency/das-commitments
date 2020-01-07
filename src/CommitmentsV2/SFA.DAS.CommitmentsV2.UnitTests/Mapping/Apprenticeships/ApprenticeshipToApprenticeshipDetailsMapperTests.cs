using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
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

            result.ApprenticeshipId.Should().Be(source.Id);
            result.ApprenticeFirstName.Should().Be(source.FirstName);
            result.ApprenticeLastName.Should().Be(source.LastName);
            result.CourseName.Should().Be(source.CourseName);
            result.EmployerName.Should().Be(source.Cohort.LegalEntityName);
            result.PlannedStartDate.Should().Be(source.StartDate.Value);
            result.PlannedEndDateTime.Should().Be(source.EndDate.Value);
            result.PaymentStatus.Should().Be(source.PaymentStatus);
            result.Uln.Should().Be(source.Uln);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Adds_Alerts_From_AlertsMapper(
            Apprenticeship source,
            List<string> alerts,
            [Frozen] Mock<IAlertsMapper> mockAlertsMapper,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            mockAlertsMapper
                .Setup(alertsMapper => alertsMapper.Map(source))
                .Returns(alerts);

            var result = await mapper.Map(source);

            result.Alerts.Should().BeEquivalentTo(alerts);
        }
    }
}