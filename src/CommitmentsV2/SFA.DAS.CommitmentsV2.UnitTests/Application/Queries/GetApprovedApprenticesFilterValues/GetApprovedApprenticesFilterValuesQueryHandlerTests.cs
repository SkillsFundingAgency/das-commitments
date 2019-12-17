using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovedApprenticesFilterValues
{
    public class GetApprovedApprenticesFilterValuesQueryHandlerTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Employer_Names(
            GetApprovedApprenticesFilterValuesQuery query,
            List<CommitmentsV2.Models.ApprovedApprenticeship> approvedApprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprovedApprenticesFilterValuesQueryHandler handler)
        {
            approvedApprenticeships[0].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[1].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].Cohort.LegalEntityName = approvedApprenticeships[1].Cohort.LegalEntityName;

            var expectedEmployerNames = new[]
                {approvedApprenticeships[0].Cohort.LegalEntityName, approvedApprenticeships[1].Cohort.LegalEntityName};

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EmployerNames.Should().BeEquivalentTo(expectedEmployerNames);
        }
    }
}