using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovedApprenticesFilterValues
{
    public class GetApprovedApprenticesFilterValuesQueryHandlerTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Employer_Names(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> approvedApprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            approvedApprenticeships[0].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[2].Cohort.LegalEntityName = approvedApprenticeships[1].Cohort.LegalEntityName;

            var expectedEmployerNames = new[]
                {approvedApprenticeships[0].Cohort.LegalEntityName, approvedApprenticeships[1].Cohort.LegalEntityName};

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EmployerNames.Should().BeEquivalentTo(expectedEmployerNames);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Course_Names(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> approvedApprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            approvedApprenticeships[0].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[2].CourseName = approvedApprenticeships[1].CourseName;

            var expectedCourseNames = new[]
                {approvedApprenticeships[0].CourseName, approvedApprenticeships[1].CourseName};

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.CourseNames.Should().BeEquivalentTo(expectedCourseNames);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Planned_Start_Dates(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> approvedApprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            approvedApprenticeships[0].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[2].StartDate = approvedApprenticeships[1].StartDate;

            var expectedStartDates = new[]
            {
                approvedApprenticeships[0].StartDate.Value,
                approvedApprenticeships[1].StartDate.Value
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.StartDates.Should().BeEquivalentTo(expectedStartDates);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Planned_End_Dates(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> approvedApprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            approvedApprenticeships[0].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = query.ProviderId;
            approvedApprenticeships[2].EndDate = approvedApprenticeships[1].EndDate;

            var expectedEndDates = new[]
            {
                approvedApprenticeships[0].EndDate.Value,
                approvedApprenticeships[1].EndDate.Value
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EndDates.Should().BeEquivalentTo(expectedEndDates);
        }

        [Test,RecursiveMoqAutoData]
        public async Task Then_Adds_Result_To_Cache_For_One_Minute(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> approvedApprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            //Arrange
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            SetupEmptyCache(query, cacheStorageService);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            cacheStorageService.Verify(x=>x.SaveToCache($"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.ProviderId}", actual, 1), Times.Once);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Checks_The_Caches_For_That_Providers_Filter_Values_And_Returns_If_Exists(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> approvedApprenticeships,
            GetApprenticeshipsFilterValuesQueryResult queryResult,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            //Arrange
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            cacheStorageService
                .Setup(x => x.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(
                    $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.ProviderId}"))
                .ReturnsAsync(queryResult);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            mockContext.Verify(x => x.Apprenticeships, Times.Never);
            actual.Should().BeEquivalentTo(queryResult);
        }

        private static void SetupEmptyCache(GetApprenticeshipsFilterValuesQuery query, Mock<ICacheStorageService> cacheStorageService)
        {
            cacheStorageService
                .Setup(x => x.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(
                    $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.ProviderId}"))
                .ReturnsAsync((GetApprenticeshipsFilterValuesQueryResult) null);
        }
    }
}