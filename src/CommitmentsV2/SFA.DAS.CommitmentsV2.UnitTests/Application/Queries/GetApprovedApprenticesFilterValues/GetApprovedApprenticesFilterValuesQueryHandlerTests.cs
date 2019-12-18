using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;
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

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Course_Names(
            GetApprovedApprenticesFilterValuesQuery query,
            List<CommitmentsV2.Models.ApprovedApprenticeship> approvedApprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprovedApprenticesFilterValuesQueryHandler handler)
        {
            approvedApprenticeships[0].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[1].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].CourseName = approvedApprenticeships[1].CourseName;

            var expectedCourseNames = new[]
                {approvedApprenticeships[0].CourseName, approvedApprenticeships[1].CourseName};

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.CourseNames.Should().BeEquivalentTo(expectedCourseNames);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Statuses(
            GetApprovedApprenticesFilterValuesQuery query,
            List<CommitmentsV2.Models.ApprovedApprenticeship> approvedApprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprovedApprenticesFilterValuesQueryHandler handler)
        {
            approvedApprenticeships[0].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[1].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].Cohort.CommitmentStatus = approvedApprenticeships[1].Cohort.CommitmentStatus;

            var expectedStatuses = new[]
                {
                    Enum.GetName(typeof(CommitmentStatus), approvedApprenticeships[0].Cohort.CommitmentStatus),
                    Enum.GetName(typeof(CommitmentStatus), approvedApprenticeships[1].Cohort.CommitmentStatus)
                };

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Statuses.Should().BeEquivalentTo(expectedStatuses);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Planned_Start_Dates(
            GetApprovedApprenticesFilterValuesQuery query,
            List<CommitmentsV2.Models.ApprovedApprenticeship> approvedApprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprovedApprenticesFilterValuesQueryHandler handler)
        {
            approvedApprenticeships[0].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[1].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].StartDate = approvedApprenticeships[1].StartDate;

            var expectedStartDates = new[]
            {
                approvedApprenticeships[0].StartDate.Value.ToString("dd/MM/yyyy"),
                approvedApprenticeships[1].StartDate.Value.ToString("dd/MM/yyyy")
            };

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.PlannedStartDates.Should().BeEquivalentTo(expectedStartDates);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Planned_End_Dates(
            GetApprovedApprenticesFilterValuesQuery query,
            List<CommitmentsV2.Models.ApprovedApprenticeship> approvedApprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprovedApprenticesFilterValuesQueryHandler handler)
        {
            approvedApprenticeships[0].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[1].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].ProviderRef = query.ProviderId.ToString();
            approvedApprenticeships[2].EndDate = approvedApprenticeships[1].EndDate;

            var expectedEndDates = new[]
            {
                approvedApprenticeships[0].EndDate.Value.ToString("dd/MM/yyyy"),
                approvedApprenticeships[1].EndDate.Value.ToString("dd/MM/yyyy")
            };

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.PlannedEndDates.Should().BeEquivalentTo(expectedEndDates);
        }
    }
}