using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.SearchTests
{
    public class WhenGettingUnsortedApprenticeships : SearchParameterServiceTestBase
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Provider_Apprenticeships(
            ApprenticeshipSearchParameters searchParameters,
            List<Apprenticeship> apprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            ApprenticeshipSearchService service)
        {
            searchParameters.PageNumber = 1;
            searchParameters.PageItemCount = 10;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.EmployerAccountId = null;

            apprenticeships[0].Cohort.ProviderId = searchParameters.ProviderId ?? 0;
            apprenticeships[1].Cohort.ProviderId = searchParameters.ProviderId ?? 0;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var expectedApprenticeships =
                apprenticeships.Where(app => app.Cohort.ProviderId == searchParameters.ProviderId);

            var result = await service.Find(searchParameters);

            result.Apprenticeships.Count()
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == searchParameters.ProviderId));

            result.Apprenticeships.Should().BeEquivalentTo(expectedApprenticeships);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Employer_Apprenticeships(
            ApprenticeshipSearchParameters searchParameters,
            List<Apprenticeship> apprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            ApprenticeshipSearchService service)
        {
            searchParameters.PageNumber = 1;
            searchParameters.PageItemCount = 10;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.ProviderId = null;

            apprenticeships[0].Cohort.EmployerAccountId = searchParameters.EmployerAccountId.Value;
            apprenticeships[1].Cohort.EmployerAccountId = searchParameters.EmployerAccountId.Value;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var expectedApprenticeships =
                apprenticeships.Where(app => app.Cohort.EmployerAccountId == searchParameters.EmployerAccountId);

            var result = await service.Find(searchParameters);

            result.Apprenticeships.Count()
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.EmployerAccountId == searchParameters.EmployerAccountId));

            result.Apprenticeships.Should().BeEquivalentTo(expectedApprenticeships);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Apprenticeships_Total_Found(
            ApprenticeshipSearchParameters searchParameters,
            List<Apprenticeship> apprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            ApprenticeshipSearchService service)
        {
            searchParameters.PageNumber = 1;
            searchParameters.PageItemCount = 10;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            apprenticeships[0].Cohort.ProviderId = searchParameters.ProviderId ?? 0;
            apprenticeships[1].Cohort.ProviderId = searchParameters.ProviderId ?? 0;
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await service.Find(searchParameters);

            result.TotalApprenticeshipsFound
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == searchParameters.ProviderId));
        }

        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_Then_Apprentices_Are_Default_Sorted(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange

            searchParameters.ReverseSort = false;
            searchParameters.PageNumber = 1;
            searchParameters.PageItemCount = 10;
            searchParameters.EmployerAccountId = null;

            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "Should_Be_Third",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("XX")},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("XX")},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Should_Be_Fourth")},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "XX",
                    CourseName = "Should_Be_Fifth",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("XX")},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                },
                new Apprenticeship
                {
                    FirstName = "Should_Be_First",
                    LastName = "XX",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("XX")},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Second",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("XX")},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.Apprenticeships.ElementAt(0).FirstName, Is.EqualTo("Should_Be_First"));
            Assert.That(actual.Apprenticeships.ElementAt(1).LastName, Is.EqualTo("Should_Be_Second"));
            Assert.That(actual.Apprenticeships.ElementAt(2).Uln, Is.EqualTo("Should_Be_Third"));
            Assert.That(actual.Apprenticeships.ElementAt(3).Cohort.AccountLegalEntity.Name, Is.EqualTo("Should_Be_Fourth"));
            Assert.That(actual.Apprenticeships.ElementAt(4).CourseName, Is.EqualTo("Should_Be_Fifth"));
            Assert.That(actual.Apprenticeships.ElementAt(5).StartDate.Value.ToString("d"), Is.EqualTo(DateTime.UtcNow.AddMonths(2).ToString("d")));
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Return_Per_Page(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.Apprenticeships.Count(), Is.EqualTo(2));
            Assert.That(actual.Apprenticeships.ElementAt(0).FirstName, Is.EqualTo("C"));
            Assert.That(actual.Apprenticeships.ElementAt(1).FirstName, Is.EqualTo("D"));
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Total_Found_Are_Return_With_Page_Data(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.TotalApprenticeshipsFound, Is.EqualTo(apprenticeships.Count));
        }

        [Test, MoqAutoData]
        public async Task Then_Total_Apprentices_Available_Will_Be_Return_For_Provider_When_Getting_Paged_Results(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);
            apprenticeships[0].Cohort.ProviderId = 0;
            apprenticeships[0].ProviderRef = null;
            searchParameters.EmployerAccountId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.TotalAvailableApprenticeships, Is.EqualTo(5));
        }

        [Test, MoqAutoData]
        public async Task Then_Total_Apprentices_Available_Will_Be_Return_For_Employer_When_Getting_Paged_Results(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);
            apprenticeships[0].Cohort.EmployerAccountId = 0;
            apprenticeships[0].EmployerRef = null;
            searchParameters.ProviderId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.TotalAvailableApprenticeships, Is.EqualTo(5));
        }

        [Test, MoqAutoData]
        public async Task Then_Total_Apprentices_Available_Will_Be_Return_For_Provider_When_Getting_All_Results(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 2;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);
            apprenticeships[0].Cohort.ProviderId = 0;
            apprenticeships[0].ProviderRef = null;
            searchParameters.EmployerAccountId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.TotalAvailableApprenticeships, Is.EqualTo(5));
        }

        [Test, MoqAutoData]
        public async Task Then_Total_Apprentices_Available_Will_Be_Return_For_Employer_When_Getting_All_Results(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 2;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);
            apprenticeships[0].Cohort.EmployerAccountId = 0;
            apprenticeships[0].EmployerRef = null;
            searchParameters.ProviderId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.TotalAvailableApprenticeships, Is.EqualTo(5));
        }


        [Test, MoqAutoData]
        public async Task Then_Apprentices_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.TotalApprenticeshipsWithAlertsFound, Is.EqualTo(3));
        }


        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_And_Is_And_There_Are_ApprenticeshipUpdates_These_Appear_First(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();


            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Second",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "ABC",
                    Cohort = new Cohort(),
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
                    {
                        new ApprenticeshipUpdate
                        {
                            Status = (byte) ApprenticeshipUpdateStatus.Pending,
                            Originator = (byte) Originator.Employer
                        }
                    }
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_First",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "ABC",
                    Cohort = new Cohort(),
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Third",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "ABC",
                    Cohort = new Cohort(),
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
                    {
                        new ApprenticeshipUpdate
                        {
                            Status = (byte) ApprenticeshipUpdateStatus.Pending,
                            Originator = Originator.Provider
                        }
                    }
                },
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.Apprenticeships.ElementAt(0).LastName, Is.EqualTo("Should_Be_Second"));
            Assert.That(actual.Apprenticeships.ElementAt(1).LastName, Is.EqualTo("Should_Be_Third"));
            Assert.That(actual.Apprenticeships.ElementAt(2).LastName, Is.EqualTo("Should_Be_First"));
        }

        [Test, MoqAutoData]
        public async Task Then_Will_Return_The_Last_Page_Number_If_Page_Number_Exceeds_Limit(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 10;
            searchParameters.PageItemCount = 5;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.ReverseSort = false;

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);
            searchParameters.ProviderId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.PageNumber, Is.EqualTo(2));
            Assert.That(actual.Apprenticeships, Is.Not.Empty);
        }


        [Test, MoqAutoData]
        public async Task Then_Will_Return_Page_Number_If_All_Apprenticeships_Are_With_Alerts(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.ReverseSort = false;
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);
            searchParameters.ProviderId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.PageNumber, Is.EqualTo(searchParameters.PageNumber));
            Assert.That(actual.Apprenticeships, Is.Not.Empty);
        }

        [Test, MoqAutoData]
        public async Task Then_Only_Apprentices_With_Active_DataLock_Alerts_Total_Found_Are_Return_With_Page_Data(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            var withExpiredDataLock = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "A",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    EmployerRef = searchParameters.EmployerAccountId.ToString(),
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>
                    {
                        new DataLockStatus {IsResolved = false, Status = Status.Fail, EventStatus = EventStatus.New, IsExpired = true}
                    }
                },
                new Apprenticeship
                {
                    FirstName = "B",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    EmployerRef = searchParameters.EmployerAccountId.ToString(),
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>
                    {
                        new DataLockStatus{IsResolved = false, Status = Status.Fail, EventStatus = EventStatus.New, IsExpired = true}
                    }
                },
                new Apprenticeship
                {
                    FirstName = "C",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    EmployerRef = searchParameters.EmployerAccountId.ToString(),
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>
                    {
                        new DataLockStatus {IsResolved = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Change, IsExpired = true}
                    }
                },
            };

           apprenticeships.AddRange(withExpiredDataLock);

           if (searchParameters.ProviderId.HasValue)
           {
               AssignProviderToApprenticeships(searchParameters.ProviderId.Value, apprenticeships);
           }

           if (searchParameters.EmployerAccountId.HasValue)
           {
               AssignEmployerToApprenticeships(searchParameters.EmployerAccountId.Value, apprenticeships);
           }

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.TotalApprenticeshipsWithAlertsFound, Is.EqualTo(3));
        }


    }
}
