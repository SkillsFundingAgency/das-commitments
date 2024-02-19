using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Apprenticeships
{
    public class ApprenticeshipToSupportApprenticeshipDetailsMapperTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Maps_Apprenticeship_To_SupportApprenticeshipDetails(
            Apprenticeship source,
            decimal cost,
            ApprenticeshipToSupportApprenticeshipDetailsMapperMapper mapper)
        {
            source.PriceHistory = new List<PriceHistory>{new PriceHistory
            {
                ApprenticeshipId = source.Id,
                Cost = cost,
                ToDate = null,
                FromDate = DateTime.UtcNow.AddMonths(-1)
            }};

            var result = await mapper.Map(source);

            result.Id.Should().Be(source.Id);
            result.FirstName.Should().Be(source.FirstName);
            result.LastName.Should().Be(source.LastName);
            result.Email.Should().Be(source.Email);
            result.CourseName.Should().Be(source.CourseName);
            result.EmployerName.Should().Be(source.Cohort.AccountLegalEntity.Name);
            result.ProviderName.Should().Be(source.Cohort.Provider.Name);
            result.StartDate.Should().Be(source.StartDate.Value);
            result.EndDate.Should().Be(source.EndDate.Value);
            result.PauseDate.Should().Be(source.PauseDate.Value);
            result.PaymentStatus.Should().Be(source.PaymentStatus);
            result.Uln.Should().Be(source.Uln);
            result.DateOfBirth.Should().Be(source.DateOfBirth.Value);
            result.ProviderRef.Should().Be(source.ProviderRef);
            result.EmployerRef.Should().Be(source.EmployerRef);
            result.TotalAgreedPrice.Should().Be(cost);
            result.CohortReference.Should().Be(source.Cohort.Reference);
            result.AccountLegalEntityId.Should().Be(source.Cohort.AccountLegalEntityId);
        }

    }
}