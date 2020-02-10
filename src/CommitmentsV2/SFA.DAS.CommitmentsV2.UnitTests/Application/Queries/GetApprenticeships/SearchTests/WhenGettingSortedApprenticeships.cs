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
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.SearchTests
{
    public class WhenGettingSortedApprenticeships : SearchParameterServiceTestBase
    {
        [Test, MoqAutoData]
        public async Task Then_Sorted_Apprentices_Are_Return_Per_Page(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "CC_Should_Be_Third_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "AA_Should_Be_First_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "AA_Should_Be_First_Name",
                    LastName = "Fog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

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
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "CC_Should_Be_Third_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "AA_Should_Be_First_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

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
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "BB_Should_Be_Second_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "CC_Should_Be_Third_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "AA_Should_Be_First_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new OrderedApprenticeshipSearchService(mockContext.Object);
            
            searchParameters.FieldName = nameof(Apprenticeship.Cohort.LegalEntityName);
            searchParameters.ReverseSort = false;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Employer", actual.Apprenticeships.ElementAt(0).Cohort.LegalEntityName);
            Assert.AreEqual("BB_Should_Be_Second_Employer", actual.Apprenticeships.ElementAt(1).Cohort.LegalEntityName);
            Assert.AreEqual("CC_Should_Be_Third_Employer", actual.Apprenticeships.ElementAt(2).Cohort.LegalEntityName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Course_Name(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "CC_Should_Be_Third_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "AA_Should_Be_First_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

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
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

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
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

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
        public async Task Then_Apprentices_Are_Sorted_By_Apprenticeship_Status(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Paused
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(3),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Completed
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(-1),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Active
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            var service = new OrderedApprenticeshipSearchService(mockContext.Object);
            
            searchParameters.FieldName = nameof(Apprenticeship.ApprenticeshipStatus);
            searchParameters.ReverseSort = false;

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_First_Sorted_By_Alerts(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
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
                    Cohort = new Cohort{LegalEntityName = "Employer"},
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
                    Cohort = new Cohort{LegalEntityName = "Employer"},
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
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PendingUpdateOriginator = null
                }
            };

            apprenticeships[0].Cohort.ProviderId = searchParameters.ProviderId;
            apprenticeships[1].Cohort.ProviderId = searchParameters.ProviderId;
            apprenticeships[2].Cohort.ProviderId = searchParameters.ProviderId;

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
    }
}
