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
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;
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
            Assert.That(actual.Apprenticeships.Count(), Is.EqualTo(apprenticeships.Count - 1));
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
            Assert.That(actual.Apprenticeships.Count(), Is.EqualTo(apprenticeships.Count - 1));
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
            Assert.That(actual.Apprenticeships.Count(), Is.EqualTo(2));
            Assert.That(actual.Apprenticeships.ElementAt(0).FirstName, Is.EqualTo("C"));
            Assert.That(actual.Apprenticeships.ElementAt(1).FirstName, Is.EqualTo("D"));
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
            Assert.That(actual.TotalApprenticeshipsWithAlertsFound, Is.EqualTo(3));
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
            Assert.That(actual.Apprenticeships.ElementAt(0).FirstName, Is.EqualTo("AA_Should_Be_First_Name"));
            Assert.That(actual.Apprenticeships.ElementAt(0).LastName, Is.EqualTo("Fog"));
            Assert.That(actual.Apprenticeships.ElementAt(1).FirstName, Is.EqualTo("AA_Should_Be_First_Name"));
            Assert.That(actual.Apprenticeships.ElementAt(1).LastName, Is.EqualTo("Zog"));
            Assert.That(actual.Apprenticeships.ElementAt(2).FirstName, Is.EqualTo("BB_Should_Be_Second_Name"));
            Assert.That(actual.Apprenticeships.ElementAt(3).FirstName, Is.EqualTo("CC_Should_Be_Third_Name"));
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
            Assert.That(actual.Apprenticeships.ElementAt(0).Uln, Is.EqualTo("AA_Should_Be_First_Uln"));
            Assert.That(actual.Apprenticeships.ElementAt(1).Uln, Is.EqualTo("BB_Should_Be_Second_Uln"));
            Assert.That(actual.Apprenticeships.ElementAt(2).Uln, Is.EqualTo("CC_Should_Be_Third_Uln"));
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
            Assert.That(actual.Apprenticeships.ElementAt(0).Cohort.AccountLegalEntity.Name, Is.EqualTo("AA_Should_Be_First_Employer"));
            Assert.That(actual.Apprenticeships.ElementAt(1).Cohort.AccountLegalEntity.Name, Is.EqualTo("BB_Should_Be_Second_Employer"));
            Assert.That(actual.Apprenticeships.ElementAt(2).Cohort.AccountLegalEntity.Name, Is.EqualTo("CC_Should_Be_Third_Employer"));
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
            Assert.That(actual.Apprenticeships.ElementAt(0).CourseName, Is.EqualTo("AA_Should_Be_First_Course"));
            Assert.That(actual.Apprenticeships.ElementAt(1).CourseName, Is.EqualTo("BB_Should_Be_Second_Course"));
            Assert.That(actual.Apprenticeships.ElementAt(2).CourseName, Is.EqualTo("CC_Should_Be_Third_Course"));
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
            Assert.That(actual.Apprenticeships.ElementAt(0).LastName, Is.EqualTo("Should_Be_First"));
            Assert.That(actual.Apprenticeships.ElementAt(1).LastName, Is.EqualTo("Should_Be_Second"));
            Assert.That(actual.Apprenticeships.ElementAt(2).LastName, Is.EqualTo("Should_Be_Third"));
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
            Assert.That(actual.Apprenticeships.ElementAt(0).LastName, Is.EqualTo("Should_Be_First"));
            Assert.That(actual.Apprenticeships.ElementAt(1).LastName, Is.EqualTo("Should_Be_Second"));
            Assert.That(actual.Apprenticeships.ElementAt(2).LastName, Is.EqualTo("Should_Be_Third"));
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
            Assert.That(actual.Apprenticeships.ElementAt(0).Cohort.Provider.Name, Is.EqualTo("Should_Be_First"));
            Assert.That(actual.Apprenticeships.ElementAt(1).Cohort.Provider.Name, Is.EqualTo("Should_Be_Second"));
            Assert.That(actual.Apprenticeships.ElementAt(2).Cohort.Provider.Name, Is.EqualTo("Should_Be_Third"));
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
            Assert.That(actual.Apprenticeships.ElementAt(0).LastName, Is.EqualTo("Should_Be_Second"));
            Assert.That(actual.Apprenticeships.ElementAt(1).LastName, Is.EqualTo("Should_Be_Third"));
            Assert.That(actual.Apprenticeships.ElementAt(2).LastName, Is.EqualTo("Should_Be_First"));
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
            Assert.That(actual.TotalAvailableApprenticeships, Is.EqualTo(5));
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
            Assert.That(actual.TotalAvailableApprenticeships, Is.EqualTo(5));
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
            Assert.That(actual.PageNumber, Is.EqualTo(2));
            Assert.IsNotEmpty(actual.Apprenticeships);
        }
    }
}
