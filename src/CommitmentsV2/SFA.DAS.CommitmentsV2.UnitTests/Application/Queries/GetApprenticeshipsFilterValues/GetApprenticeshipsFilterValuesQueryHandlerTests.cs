using System;
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
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQueryHandlerTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Employer_Names_For_Provider(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId);
            apprenticeships[0].Cohort.AccountLegalEntity = CreateAccountLegalEntity("test");
            apprenticeships[1].Cohort.AccountLegalEntity = CreateAccountLegalEntity("test2");
            apprenticeships[2].Cohort.AccountLegalEntity = apprenticeships[1].Cohort.AccountLegalEntity;

            var expectedEmployerNames = new[]
                {apprenticeships[0].Cohort.AccountLegalEntity.Name, apprenticeships[1].Cohort.AccountLegalEntity.Name};

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EmployerNames.Should().BeEquivalentTo(expectedEmployerNames);
            result.ProviderNames.Should().BeNullOrEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Provider_Names_For_Employer(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.ProviderId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetEmployerIdOnApprenticeship(apprenticeships, query.EmployerAccountId);
            apprenticeships[2].Cohort.Provider = apprenticeships[1].Cohort.Provider;

            var expectedProviderNames = new[]
                {apprenticeships[0].Cohort.Provider.Name, apprenticeships[1].Cohort.Provider.Name};

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.ProviderNames.Should().BeEquivalentTo(expectedProviderNames);
            result.EmployerNames.Should().BeNullOrEmpty();
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Course_Names_For_Provider(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId);

            apprenticeships[2].CourseName = apprenticeships[1].CourseName;

            var expectedCourseNames = new[]
                {apprenticeships[0].CourseName, apprenticeships[1].CourseName};

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.CourseNames.Should().BeEquivalentTo(expectedCourseNames);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Course_Names_For_Employer(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.ProviderId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetEmployerIdOnApprenticeship(apprenticeships, query.EmployerAccountId);

            apprenticeships[2].CourseName = apprenticeships[1].CourseName;

            var expectedCourseNames = new[]
                {apprenticeships[0].CourseName, apprenticeships[1].CourseName};

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.CourseNames.Should().BeEquivalentTo(expectedCourseNames);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Planned_Start_Dates_For_Provider(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId);

            apprenticeships[2].StartDate = apprenticeships[1].StartDate;

            var expectedStartDates = new[]
            {
                apprenticeships[0].StartDate.Value,
                apprenticeships[1].StartDate.Value
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.StartDates.Should().BeEquivalentTo(expectedStartDates);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Planned_Start_Dates_For_Employer(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.ProviderId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetEmployerIdOnApprenticeship(apprenticeships, query.EmployerAccountId);

            apprenticeships[2].StartDate = apprenticeships[1].StartDate;

            var expectedStartDates = new[]
            {
                apprenticeships[0].StartDate.Value,
                apprenticeships[1].StartDate.Value
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.StartDates.Should().BeEquivalentTo(expectedStartDates);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Planned_End_Dates_For_Provider(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId);

            apprenticeships[2].EndDate = apprenticeships[1].EndDate;

            var expectedEndDates = new[]
            {
                apprenticeships[0].EndDate.Value,
                apprenticeships[1].EndDate.Value
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EndDates.Should().BeEquivalentTo(expectedEndDates);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Planned_End_Dates_For_Employer(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.ProviderId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetEmployerIdOnApprenticeship(apprenticeships, query.EmployerAccountId);

            apprenticeships[2].EndDate = apprenticeships[1].EndDate;

            var expectedEndDates = new[]
            {
                apprenticeships[0].EndDate.Value,
                apprenticeships[1].EndDate.Value
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EndDates.Should().BeEquivalentTo(expectedEndDates);
        }

        [Test,RecursiveMoqAutoData]
        public async Task Then_Adds_Result_To_Cache_For_One_Minute_For_Provider(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;

            //Arrange
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            SetupEmptyCache(query, cacheStorageService);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            cacheStorageService.Verify(x=>x.SaveToCache($"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.ProviderId}", actual, 1), Times.Once);
        }

        [Test,RecursiveMoqAutoData]
        public async Task Then_Adds_Result_To_Cache_For_One_Minute_For_Employer(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.ProviderId = null;

            //Arrange
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            SetupEmptyCache(query, cacheStorageService);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            cacheStorageService.Verify(x=>x.SaveToCache($"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.EmployerAccountId}", actual, 1), Times.Once);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Checks_The_Caches_For_That_Providers_Filter_Values_And_Returns_If_Exists(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            GetApprenticeshipsFilterValuesQueryResult queryResult,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;

            //Arrange
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
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

        [Test, RecursiveMoqAutoData]
        public async Task Then_Checks_The_Caches_For_That_Employers_Filter_Values_And_Returns_If_Exists(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            GetApprenticeshipsFilterValuesQueryResult queryResult,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.ProviderId = null;

            //Arrange
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            cacheStorageService
                .Setup(x => x.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(
                    $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.EmployerAccountId}"))
                .ReturnsAsync(queryResult);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            mockContext.Verify(x => x.Apprenticeships, Times.Never);
            actual.Should().BeEquivalentTo(queryResult);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Ordered_EmployerNames(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId ?? 0);

            apprenticeships[0].Cohort.AccountLegalEntity = CreateAccountLegalEntity("B");
            apprenticeships[1].Cohort.AccountLegalEntity = CreateAccountLegalEntity("C");
            apprenticeships[2].Cohort.AccountLegalEntity = CreateAccountLegalEntity("A");

            var expectedEmployerNames = new[]
            {
                "A",
                "B",
                "C"
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EmployerNames.Should().BeEquivalentTo(expectedEmployerNames);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Ordered_CourseNames(
            GetApprenticeshipsFilterValuesQuery query,
            List<Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;

            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId ?? 0);
            query.EmployerAccountId = null;
            apprenticeships[0].CourseName = "B";
            apprenticeships[1].CourseName= "C";
            apprenticeships[2].CourseName= "A";

            var expectedCourseNames = new[]
            {
                apprenticeships[2].CourseName,
                apprenticeships[0].CourseName,
                apprenticeships[1].CourseName
            };
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            var result = await handler.Handle(query, CancellationToken.None);

            result.CourseNames.Should().BeEquivalentTo(expectedCourseNames);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Ordered_PlannedStartDates(
            GetApprenticeshipsFilterValuesQuery query,
            List<Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId ?? 0);
                
            var now = DateTime.UtcNow;

            apprenticeships[0].StartDate = now.AddMonths(-1);
            apprenticeships[1].StartDate = now.AddMonths(-2);
            apprenticeships[2].StartDate = now;
            var expectedStartDates = new[]
            {
                apprenticeships[2].StartDate,
                apprenticeships[0].StartDate,
                apprenticeships[1].StartDate
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.StartDates.Should().BeEquivalentTo(expectedStartDates);
        }
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Ordered_PlannedEndDates(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId ?? 0);

            var now = DateTime.UtcNow;

            apprenticeships[0].EndDate = now.AddMonths(-1);
            apprenticeships[1].EndDate = now.AddMonths(-2);
            apprenticeships[2].EndDate = now;

            var expectedEndDates = new[]
            {
                apprenticeships[2].EndDate,
                apprenticeships[0].EndDate,
                apprenticeships[1].EndDate
            };

            mockContext.Setup(context => context.Apprenticeships).ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);
            result.EndDates.Should().BeEquivalentTo(expectedEndDates);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Ordered_LegalEntityNames(
            GetApprenticeshipsFilterValuesQuery query,
            List<Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            query.EmployerAccountId = null;
            SetupEmptyCache(query, cacheStorageService);
            SetProviderIdOnApprenticeship(apprenticeships, query.ProviderId ?? 0);

            apprenticeships[0].Cohort.AccountLegalEntity = CreateAccountLegalEntity("B");
            apprenticeships[1].Cohort.AccountLegalEntity = CreateAccountLegalEntity("C");
            apprenticeships[2].Cohort.AccountLegalEntity = CreateAccountLegalEntity("A");

            var expectedEmployerNames = new[]
            {
                "A",
                "B",
                "C"
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EmployerNames.Should().BeEquivalentTo(expectedEmployerNames);
        }

        private void SetProviderIdOnApprenticeship(IList<CommitmentsV2.Models.Apprenticeship> apprenticeships,long? providerId)
        {
            var providerIdValue = providerId.GetValueOrDefault();

            apprenticeships[0].Cohort.ProviderId = providerIdValue;
            apprenticeships[1].Cohort.ProviderId = providerIdValue;
            apprenticeships[2].Cohort.ProviderId = providerIdValue;
        }

        private void SetEmployerIdOnApprenticeship(IList<CommitmentsV2.Models.Apprenticeship> apprenticeships, long? employerAccountId)
        {
            var employerIdValue = employerAccountId.GetValueOrDefault();

            apprenticeships[0].Cohort.EmployerAccountId = employerIdValue;
            apprenticeships[1].Cohort.EmployerAccountId = employerIdValue;
            apprenticeships[2].Cohort.EmployerAccountId = employerIdValue;
        }

        private static void SetupEmptyCache(GetApprenticeshipsFilterValuesQuery query, Mock<ICacheStorageService> cacheStorageService)
        {
            cacheStorageService
                .Setup(x => x.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(
                    $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.ProviderId}"))
                .ReturnsAsync((GetApprenticeshipsFilterValuesQueryResult) null);

            cacheStorageService
                .Setup(x => x.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(
                    $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.EmployerAccountId}"))
                .ReturnsAsync((GetApprenticeshipsFilterValuesQueryResult) null);
        }

        private AccountLegalEntity CreateAccountLegalEntity(string name)
        {
            var account = new Account(1, "", "", name, DateTime.UtcNow);
            return new AccountLegalEntity(account, 1, 1, "", "", name, OrganisationType.CompaniesHouse, "",
                DateTime.UtcNow);
        }
    }
}