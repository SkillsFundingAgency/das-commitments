using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models
{
    public class ApprovedApprenticeshipTests
    {
        [Test, RecursiveMoqAutoData]
        public void Then_Casts_ApprovedApprenticeship_To_ApprenticeshipDetails(
            ApprovedApprenticeship source)
        {
            ApprenticeshipDetails result = source;

            result.ApprenticeFirstName.Should().Be(source.FirstName);
            result.ApprenticeLastName.Should().Be(source.LastName);
            result.CourseName.Should().Be(source.CourseName);
            result.EmployerName.Should().Be(source.Cohort.LegalEntityName);
            result.PlannedStartDate.Should().Be(source.StartDate.Value);
            result.PlannedEndDateTime.Should().Be(source.EndDate.Value);
            result.PaymentStatus.Should().Be(source.PaymentStatus);
            result.Uln.Should().Be(source.Uln);
            //todo: datalockstatus
        }
    }
}