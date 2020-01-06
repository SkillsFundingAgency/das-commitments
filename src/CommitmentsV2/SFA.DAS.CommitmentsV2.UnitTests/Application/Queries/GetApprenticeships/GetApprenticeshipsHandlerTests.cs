using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsHandlerTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Apprenticeships(
            GetApprenticeshipsRequest request,
            List<Apprenticeship> apprenticeships,
            ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsHandler handler)
        {
            apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            mockMapper
                .Setup(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId))))
                .ReturnsAsync(apprenticeshipDetails);

            var result = await handler.Handle(request, CancellationToken.None);

            result.Apprenticeships.Count()
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId));
            result.Apprenticeships.Should().AllBeEquivalentTo(apprenticeshipDetails);
        }

        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_Then_Apprentices_Are_Default_Sorted(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "Should_Be_Third",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Should_Be_Fourth"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "XX",
                    CourseName = "Should_Be_Fifth",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "Should_Be_First",
                    LastName = "XX",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Second",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };
            apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            apprenticeships[2].Cohort.ProviderId = request.ProviderId;
            apprenticeships[3].Cohort.ProviderId = request.ProviderId;
            apprenticeships[4].Cohort.ProviderId = request.ProviderId;
            apprenticeships[5].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);
            request.SortField = "";

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).ApprenticeFirstName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).Uln);
            Assert.AreEqual("Should_Be_Fourth", actual.Apprenticeships.ElementAt(3).EmployerName);
            Assert.AreEqual("Should_Be_Fifth", actual.Apprenticeships.ElementAt(4).CourseName);
            Assert.AreEqual(DateTime.UtcNow.AddMonths(2).ToString("d"), actual.Apprenticeships.ElementAt(5).PlannedStartDate.ToString("d"));
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_Name(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
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
                    FirstName = "CC_Should_Be_Third_Name",
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
                    FirstName = "AA_Should_Be_First_Name",
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
                    FirstName = "AA_Should_Be_First_Name",
                    LastName = "Fog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };
            apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            apprenticeships[2].Cohort.ProviderId = request.ProviderId;
            apprenticeships[3].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);
            request.SortField = nameof(Apprenticeship.FirstName);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Name", actual.Apprenticeships.ElementAt(0).ApprenticeFirstName);
            Assert.AreEqual("Fog", actual.Apprenticeships.ElementAt(0).ApprenticeLastName);
            Assert.AreEqual("AA_Should_Be_First_Name", actual.Apprenticeships.ElementAt(1).ApprenticeFirstName);
            Assert.AreEqual("Zog", actual.Apprenticeships.ElementAt(1).ApprenticeLastName);
            Assert.AreEqual("BB_Should_Be_Second_Name", actual.Apprenticeships.ElementAt(2).ApprenticeFirstName);
            Assert.AreEqual("CC_Should_Be_Third_Name", actual.Apprenticeships.ElementAt(3).ApprenticeFirstName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Uln(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var Apprenticeships = new List<Apprenticeship>
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
                    Uln = "CC_Should_Be_Third_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "AA_Should_Be_First_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };
            Apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(Apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);
            request.SortField = nameof(Apprenticeship.Uln);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Uln", actual.Apprenticeships.ElementAt(0).Uln);
            Assert.AreEqual("BB_Should_Be_Second_Uln", actual.Apprenticeships.ElementAt(1).Uln);
            Assert.AreEqual("CC_Should_Be_Third_Uln", actual.Apprenticeships.ElementAt(2).Uln);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Employer_Name(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var Apprenticeships = new List<Apprenticeship>
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
                    Cohort = new Cohort{LegalEntityName = "CC_Should_Be_Third_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "AA_Should_Be_First_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };
            Apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(Apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);
            request.SortField = nameof(Apprenticeship.Cohort.LegalEntityName);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Employer", actual.Apprenticeships.ElementAt(0).EmployerName);
            Assert.AreEqual("BB_Should_Be_Second_Employer", actual.Apprenticeships.ElementAt(1).EmployerName);
            Assert.AreEqual("CC_Should_Be_Third_Employer", actual.Apprenticeships.ElementAt(2).EmployerName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Course_Name(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var Apprenticeships = new List<Apprenticeship>
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
                    CourseName = "CC_Should_Be_Third_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "AA_Should_Be_First_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };
            Apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(Apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            request.SortField = nameof(Apprenticeship.CourseName);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Course", actual.Apprenticeships.ElementAt(0).CourseName);
            Assert.AreEqual("BB_Should_Be_Second_Course", actual.Apprenticeships.ElementAt(1).CourseName);
            Assert.AreEqual("CC_Should_Be_Third_Course", actual.Apprenticeships.ElementAt(2).CourseName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Planned_Start_Date(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var Apprenticeships = new List<Apprenticeship>
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
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Third",
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
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };
            Apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(Apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);
            request.SortField = nameof(Apprenticeship.StartDate);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).ApprenticeLastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Planned_Stop_Date(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StopDate = DateTime.UtcNow.AddMonths(1),
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
                    StopDate = DateTime.UtcNow.AddMonths(2),
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
                    StopDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };
            Apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(Apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);
            request.SortField = nameof(Apprenticeship.StopDate);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).ApprenticeLastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Payment_Status(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);
            var Apprenticeships = new List<Apprenticeship>
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
                    LastName = "Should_Be_Third",
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
                    LastName = "Should_Be_First",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Active
                }
            };
            Apprenticeships[0].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[1].Cohort.ProviderId = request.ProviderId;
            Apprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(Apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);
            request.SortField = nameof(Apprenticeship.PaymentStatus);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).ApprenticeLastName);
        }
    }
}