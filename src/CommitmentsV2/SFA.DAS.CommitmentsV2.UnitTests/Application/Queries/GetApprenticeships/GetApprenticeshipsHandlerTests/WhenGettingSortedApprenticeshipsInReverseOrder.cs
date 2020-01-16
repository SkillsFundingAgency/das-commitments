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
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.GetApprenticeshipsHandlerTests
{
    public class WhenGettingSortedApprenticeshipsInReverseOrder : GetApprenticeshipsHandlerTestBase
    {
        [Test, MoqAutoData]
        public async Task And_Sorted_By_Alerts_And_Is_Reverse_Sorted_Then_Apprentices_Are_Default_Sorted(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = "DataLockStatus";
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Second",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PendingUpdateOriginator = Originator.Unknown
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_First",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PendingUpdateOriginator = Originator.Provider
                }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Reverse_Sorted_Apprentices_Are_Return_Per_Page(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);

            var apprenticeships = GetTestApprenticeships(request);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            request.SortField = nameof(Apprenticeship.FirstName);
            request.PageNumber = 2;
            request.PageItemCount = 2;

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.Apprenticeships.Count());
            Assert.AreEqual("B", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("A", actual.Apprenticeships.ElementAt(1).FirstName);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Reverse_Sorted_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);

            var apprenticeships = GetTestApprenticeships(request);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            request.SortField = nameof(Apprenticeship.FirstName);
            request.PageNumber = 2;
            request.PageItemCount = 2;

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.TotalApprenticeshipsWithAlertsFound);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_Name(
           GetApprenticeshipsRequest request,
           Mock<IAlertsMapper> alertsMapper,
           [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SortField = nameof(Apprenticeship.FirstName);
            request.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "BB_Should_Be_Second_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "CC_Should_Be_First_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "AA_Should_Be_Third_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "AA_Should_Be_Third_Name",
                    LastName = "Fog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);


            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("CC_Should_Be_First_Name", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("BB_Should_Be_Second_Name", actual.Apprenticeships.ElementAt(1).FirstName);
            Assert.AreEqual("AA_Should_Be_Third_Name", actual.Apprenticeships.ElementAt(2).FirstName);
            Assert.AreEqual("Zog", actual.Apprenticeships.ElementAt(2).LastName);
            Assert.AreEqual("AA_Should_Be_Third_Name", actual.Apprenticeships.ElementAt(3).FirstName);
            Assert.AreEqual("Fog", actual.Apprenticeships.ElementAt(3).LastName);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Uln(
           GetApprenticeshipsRequest request,
           Mock<IAlertsMapper> alertsMapper,
           [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SortField = nameof(Apprenticeship.Uln);
            request.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "BB_Should_Be_Second_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "CC_Should_Be_First_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "AA_Should_Be_Third_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("CC_Should_Be_First_Uln", actual.Apprenticeships.ElementAt(0).Uln);
            Assert.AreEqual("BB_Should_Be_Second_Uln", actual.Apprenticeships.ElementAt(1).Uln);
            Assert.AreEqual("AA_Should_Be_Third_Uln", actual.Apprenticeships.ElementAt(2).Uln);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Employer_Name(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SortField = nameof(Apprenticeship.Cohort.LegalEntityName);
            request.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "BB_Should_Be_Second_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "CC_Should_Be_First_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "AA_Should_Be_Third_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("CC_Should_Be_First_Employer", actual.Apprenticeships.ElementAt(0).EmployerName);
            Assert.AreEqual("BB_Should_Be_Second_Employer", actual.Apprenticeships.ElementAt(1).EmployerName);
            Assert.AreEqual("AA_Should_Be_Third_Employer", actual.Apprenticeships.ElementAt(2).EmployerName);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Course_Name(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SortField = nameof(Apprenticeship.CourseName);
            request.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "BB_Should_Be_Second_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "CC_Should_Be_First_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "AA_Should_Be_Third_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("CC_Should_Be_First_Course", actual.Apprenticeships.ElementAt(0).CourseName);
            Assert.AreEqual("BB_Should_Be_Second_Course", actual.Apprenticeships.ElementAt(1).CourseName);
            Assert.AreEqual("AA_Should_Be_Third_Course", actual.Apprenticeships.ElementAt(2).CourseName);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Planned_Start_Date(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SortField = nameof(Apprenticeship.StartDate);
            request.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "BB_Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "AA_Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "CC_Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);


            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("BB_Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("CC_Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Planned_End_Date(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SortField = nameof(Apprenticeship.EndDate);
            request.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    EndDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Payment_Status(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SortField = nameof(Apprenticeship.PaymentStatus);
            request.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Paused
                    },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Completed
                    },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Active
                    }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }
    }
}
