using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.GetApprenticeshipsHandlerTests
{
    public class WhenGettingSortedApprenticeships : GetApprenticeshipsHandlerTestBase
    {
        [Test, MoqAutoData]
        public async Task Then_Sorted_Apprentices_Are_Return_Per_Page(
            GetApprenticeshipsQuery query,
          
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = nameof(Apprenticeship.FirstName);
            query.PageNumber = 2;
            query.PageItemCount = 2;
            query.ReverseSort = false;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = GetTestApprenticeshipsWithAlerts(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);



            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.Apprenticeships.Count());
            Assert.AreEqual("C", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("D", actual.Apprenticeships.ElementAt(1).FirstName);
        }

        [Test, MoqAutoData]
        public async Task Then_Sorted_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = nameof(Apprenticeship.FirstName);
            query.PageNumber = 2;
            query.PageItemCount = 2;
            query.ReverseSort = false;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = GetTestApprenticeshipsWithAlerts(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(3, actual.TotalApprenticeshipsWithAlertsFound);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "BB_Should_Be_Second_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            query.SortField = nameof(Apprenticeship.FirstName);
            query.ReverseSort = false;
            query.SearchFilters = null;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

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
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "BB_Should_Be_Second_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "CC_Should_Be_Third_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "AA_Should_Be_First_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            query.SortField = nameof(Apprenticeship.Uln);
            query.ReverseSort = false;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Uln", actual.Apprenticeships.ElementAt(0).Uln);
            Assert.AreEqual("BB_Should_Be_Second_Uln", actual.Apprenticeships.ElementAt(1).Uln);
            Assert.AreEqual("CC_Should_Be_Third_Uln", actual.Apprenticeships.ElementAt(2).Uln);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Employer_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("BB_Should_Be_Second_Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("CC_Should_Be_Third_Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("AA_Should_Be_First_Employer") },
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            query.SortField = nameof(Apprenticeship.Cohort.AccountLegalEntity.Name);
            query.ReverseSort = false;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Employer", actual.Apprenticeships.ElementAt(0).EmployerName);
            Assert.AreEqual("BB_Should_Be_Second_Employer", actual.Apprenticeships.ElementAt(1).EmployerName);
            Assert.AreEqual("CC_Should_Be_Third_Employer", actual.Apprenticeships.ElementAt(2).EmployerName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Course_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "BB_Should_Be_Second_Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "CC_Should_Be_Third_Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "AA_Should_Be_First_Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            query.SortField = nameof(Apprenticeship.CourseName);
            query.ReverseSort = false;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Course", actual.Apprenticeships.ElementAt(0).CourseName);
            Assert.AreEqual("BB_Should_Be_Second_Course", actual.Apprenticeships.ElementAt(1).CourseName);
            Assert.AreEqual("CC_Should_Be_Third_Course", actual.Apprenticeships.ElementAt(2).CourseName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Planned_Start_Date(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            query.SortField = nameof(Apprenticeship.StartDate);
            query.ReverseSort = false;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Planned_End_Date(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            query.SortField = nameof(Apprenticeship.EndDate);
            query.ReverseSort = false;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Apprenticeship_Status(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Active
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            query.SortField = nameof(Apprenticeship.ApprenticeshipStatus);
            query.ReverseSort = false;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_First_Sorted_By_Alerts(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());
            var Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    DataLockStatus = new List<DataLockStatus>(),
                    PendingUpdateOriginator = null
                }
            };
            Apprenticeships[0].Cohort.ProviderId = query.ProviderId;
            Apprenticeships[1].Cohort.ProviderId = query.ProviderId;
            Apprenticeships[2].Cohort.ProviderId = query.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(Apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(2).LastName);
        }

        private AccountLegalEntity CreateAccountLegalEntity(string name)
        {
            var account = new Account(1, "", "", name, DateTime.UtcNow);
            return new AccountLegalEntity(account, 1, 1, "", "", name, OrganisationType.CompaniesHouse, "",
                DateTime.UtcNow);
        }

    }
}
