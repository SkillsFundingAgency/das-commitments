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
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQueryHandlerTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Employer_Names(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            apprenticeships[0].Cohort.ProviderId = query.ProviderId;
            apprenticeships[1].Cohort.ProviderId = query.ProviderId;
            apprenticeships[2].Cohort.ProviderId = query.ProviderId;
            apprenticeships[1].Cohort.AccountLegalEntity = CreateAccountLegalEntity("test");
            apprenticeships[2].Cohort.AccountLegalEntity = apprenticeships[1].Cohort.AccountLegalEntity;

            var expectedEmployerNames = new[]
                {apprenticeships[0].Cohort.AccountLegalEntity.Name, apprenticeships[1].Cohort.AccountLegalEntity.Name};

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.EmployerNames.Should().BeEquivalentTo(expectedEmployerNames);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_All_Distinct_Course_Names(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            apprenticeships[0].Cohort.ProviderId = query.ProviderId;
            apprenticeships[1].Cohort.ProviderId = query.ProviderId;
            apprenticeships[2].Cohort.ProviderId = query.ProviderId;
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
        public async Task Then_Returns_All_Distinct_Planned_Start_Dates(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            apprenticeships[0].Cohort.ProviderId = query.ProviderId;
            apprenticeships[1].Cohort.ProviderId = query.ProviderId;
            apprenticeships[2].Cohort.ProviderId = query.ProviderId;
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
        public async Task Then_Returns_All_Distinct_Planned_End_Dates(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
            SetupEmptyCache(query, cacheStorageService);
            apprenticeships[0].Cohort.ProviderId = query.ProviderId;
            apprenticeships[1].Cohort.ProviderId = query.ProviderId;
            apprenticeships[2].Cohort.ProviderId = query.ProviderId;
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
        public async Task Then_Adds_Result_To_Cache_For_One_Minute(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
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

        [Test, RecursiveMoqAutoData]
        public async Task Then_Checks_The_Caches_For_That_Providers_Filter_Values_And_Returns_If_Exists(
            GetApprenticeshipsFilterValuesQuery query,
            List<CommitmentsV2.Models.Apprenticeship> apprenticeships,
            GetApprenticeshipsFilterValuesQueryResult queryResult,
            [Frozen] Mock<ICacheStorageService> cacheStorageService,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprenticeshipsFilterValuesQueryHandler handler)
        {
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

        private static void SetupEmptyCache(GetApprenticeshipsFilterValuesQuery query, Mock<ICacheStorageService> cacheStorageService)
        {
            cacheStorageService
                .Setup(x => x.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(
                    $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.ProviderId}"))
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