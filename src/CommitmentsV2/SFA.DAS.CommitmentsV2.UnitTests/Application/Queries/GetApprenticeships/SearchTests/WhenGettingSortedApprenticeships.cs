using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.UnitTests.DatabaseMock;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.SearchTests
{
    public class WhenGettingSortedApprenticeships : SearchParameterServiceTestBase
    {
        [Test, MoqAutoData]
        public async Task Then_Sorted_Apprentices_For_Provider_Are_Return(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.FieldName = nameof(Apprenticeship.FirstName);
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.CancellationToken = CancellationToken.None;
            searchParameters.EmployerAccountId = null;


            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            apprenticeships[1].ProviderRef = null;
            apprenticeships[1].Cohort.ProviderId = 0;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual(apprenticeships.Count -1, actual.Apprenticeships.Count());
            Assert.IsFalse(actual.Apprenticeships.Contains(apprenticeships[1]));
        }

        [Test, MoqAutoData]
        public async Task Then_Sorted_Apprentices_For_Employer_Are_Return(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.FieldName = nameof(Apprenticeship.FirstName);
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.CancellationToken = CancellationToken.None;
            searchParameters.ProviderId = null;


            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            apprenticeships[1].EmployerRef = null;
            apprenticeships[1].Cohort.EmployerAccountId = 0;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual(apprenticeships.Count -1, actual.Apprenticeships.Count());
            Assert.IsFalse(actual.Apprenticeships.Contains(apprenticeships[1]));
        }

        [Test, MoqAutoData]
        public async Task Then_Sorted_Apprentices_Are_Return_Per_Page(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.FieldName = nameof(Apprenticeship.FirstName);
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.CancellationToken = CancellationToken.None;

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual(2, actual.Apprenticeships.Count());
            Assert.AreEqual("C", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("D", actual.Apprenticeships.ElementAt(1).FirstName);
        }

        [Test, MoqAutoData]
        public async Task Then_Sorted_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            OrderedApprenticeshipSearchParameters searchParams,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParams.FieldName = nameof(Apprenticeship.FirstName);
            searchParams.PageNumber = 2;
            searchParams.PageItemCount = 2;
            searchParams.ReverseSort = false;
            searchParams.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParams);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
           
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParams);

            //Assert
            Assert.AreEqual(3, actual.TotalApprenticeshipsWithAlertsFound);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_Name(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "BB_Should_Be_Second_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "CC_Should_Be_Third_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "AA_Should_Be_First_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "AA_Should_Be_First_Name",
                    LastName = "Fog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
         
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            searchParameters.FieldName = nameof(Apprenticeship.FirstName);
            searchParameters.ReverseSort = false;
            searchParameters.Filters = null;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Name", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("Fog", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("AA_Should_Be_First_Name", actual.Apprenticeships.ElementAt(1).FirstName);
            Assert.AreEqual("Zog", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("BB_Should_Be_Second_Name", actual.Apprenticeships.ElementAt(2).FirstName);
            Assert.AreEqual("CC_Should_Be_Third_Name", actual.Apprenticeships.ElementAt(3).FirstName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Uln(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "BB_Should_Be_Second_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "CC_Should_Be_Third_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "AA_Should_Be_First_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
           
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);
            
            searchParameters.FieldName = nameof(Apprenticeship.Uln);
            searchParameters.ReverseSort = false;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Uln", actual.Apprenticeships.ElementAt(0).Uln);
            Assert.AreEqual("BB_Should_Be_Second_Uln", actual.Apprenticeships.ElementAt(1).Uln);
            Assert.AreEqual("CC_Should_Be_Third_Uln", actual.Apprenticeships.ElementAt(2).Uln);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Employer_Name(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("BB_Should_Be_Second_Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("CC_Should_Be_Third_Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("AA_Should_Be_First_Employer") },
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //"Name" refers to nameof(Apprenticeship.Cohort.AccountLegalEntity.Name)
            searchParameters.FieldName= "Name";
            searchParameters.ReverseSort = false;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Employer", actual.Apprenticeships.ElementAt(0).Cohort.AccountLegalEntity.Name);
            Assert.AreEqual("BB_Should_Be_Second_Employer", actual.Apprenticeships.ElementAt(1).Cohort.AccountLegalEntity.Name);
            Assert.AreEqual("CC_Should_Be_Third_Employer", actual.Apprenticeships.ElementAt(2).Cohort.AccountLegalEntity.Name);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Course_Name(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "BB_Should_Be_Second_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "CC_Should_Be_Third_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "AA_Should_Be_First_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            searchParameters.FieldName = nameof(Apprenticeship.CourseName);
            searchParameters.ReverseSort = false;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Course", actual.Apprenticeships.ElementAt(0).CourseName);
            Assert.AreEqual("BB_Should_Be_Second_Course", actual.Apprenticeships.ElementAt(1).CourseName);
            Assert.AreEqual("CC_Should_Be_Third_Course", actual.Apprenticeships.ElementAt(2).CourseName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Planned_Start_Date(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);
            
            searchParameters.FieldName = nameof(Apprenticeship.StartDate);
            searchParameters.ReverseSort = false;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Planned_End_Date(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow,
                    ProviderRef = "Provider ref",
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);
           
            searchParameters.FieldName = nameof(Apprenticeship.EndDate);
            searchParameters.ReverseSort = false;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Provider_Name(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var provider1 = new Provider(searchParameters.ProviderId.Value, "Should_Be_First", DateTime.UtcNow, DateTime.UtcNow);
            var provider2 = new Provider(searchParameters.ProviderId.Value, "Should_Be_Second", DateTime.UtcNow, DateTime.UtcNow);
            var provider3 = new Provider(searchParameters.ProviderId.Value, "Should_Be_Third", DateTime.UtcNow, DateTime.UtcNow);

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName_1",
                    LastName = "LastName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{Provider = provider2, ProviderId = provider2.UkPrn},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Paused
                },
                new Apprenticeship
                {
                    FirstName = "FirstName_2",
                    LastName = "LastName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(3),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{Provider = provider3, ProviderId = provider3.UkPrn},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Completed
                },
                new Apprenticeship
                {
                    FirstName = "FirstName_3",
                    LastName = "LastName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(-1),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{Provider = provider1, ProviderId = provider1.UkPrn},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Active
                }
            };

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            searchParameters.FieldName = "ProviderName";
            searchParameters.ReverseSort = false;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).Cohort.Provider.Name);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).Cohort.Provider.Name);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).Cohort.Provider.Name);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_First_Sorted_By_Alerts(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>(),
                    PendingUpdateOriginator = Originator.Provider
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>(),
                    PendingUpdateOriginator = Originator.Provider
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>(),
                    PendingUpdateOriginator = null
                }
            };

            apprenticeships[0].Cohort.ProviderId = searchParameters.ProviderId ?? 0;
            apprenticeships[1].Cohort.ProviderId = searchParameters.ProviderId ?? 0;
            apprenticeships[2].Cohort.ProviderId = searchParameters.ProviderId ?? 0;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);
            
            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Will_Return_Total_Apprenticeships_For_Provider(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            apprenticeships[0].Cohort.ProviderId = 0;
            apprenticeships[0].ProviderRef = null;
            searchParameters.EmployerAccountId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
           
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual(5, actual.TotalAvailableApprenticeships);
        }

        [Test, MoqAutoData]
        public async Task Then_Will_Return_Total_Apprenticeships_For_Employer(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.FieldName = nameof(Apprenticeship.FirstName);
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);
            apprenticeships[0].Cohort.EmployerAccountId = 0;
            apprenticeships[0].EmployerRef = null;

            searchParameters.ProviderId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
           
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual(5, actual.TotalAvailableApprenticeships);
        }

        [Test, MoqAutoData]
        public async Task Then_Will_Return_Page_Number_Of_One_If_Only_Page(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.FieldName = nameof(Apprenticeship.FirstName);
            searchParameters.PageNumber = 20;
            searchParameters.PageItemCount = 5;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);
            searchParameters.ProviderId = null;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
           
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual(2, actual.PageNumber);
            Assert.IsNotEmpty(actual.Apprenticeships);
        }
    }
}
