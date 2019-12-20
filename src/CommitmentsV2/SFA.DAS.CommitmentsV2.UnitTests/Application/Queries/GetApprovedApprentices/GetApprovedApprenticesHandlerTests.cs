using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesHandlerTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Approved_Apprentices(
            GetApprovedApprenticesRequest request,
            List<ApprovedApprenticeship> approvedApprenticeships,
            ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<ApprovedApprenticeship, ApprenticeshipDetails>> mockMapper,
            GetApprovedApprenticesHandler handler)
        {
            approvedApprenticeships[0].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = request.ProviderId;
            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            mockMapper
                .Setup(mapper => mapper.Map(It.IsIn(approvedApprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId))))
                .ReturnsAsync(apprenticeshipDetails);

            var result = await handler.Handle(request, CancellationToken.None);

            result.Apprenticeships.Count()
                .Should().Be(approvedApprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId));
            result.Apprenticeships.Should().AllBeEquivalentTo(apprenticeshipDetails);
        }

        [Test, MoqAutoData]
        public async Task Then_Approved_Apprentices_Are_Sorted_By_First_Name(
            GetApprovedApprenticesRequest request,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var approvedApprenticeships = new List<ApprovedApprenticeship>
            {
                new ApprovedApprenticeship
                {
                    FirstName = "BB_Should_Be_Second_Name",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new ApprovedApprenticeship
                {
                    FirstName = "CC_Should_Be_Third_Name",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new ApprovedApprenticeship
                {
                    FirstName = "AA_Should_Be_First_Name",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };
            approvedApprenticeships[0].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            var handler = new GetApprovedApprenticesHandler(mockContext.Object,mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Name", actual.Apprenticeships.ElementAt(0).ApprenticeFirstName);
            Assert.AreEqual("BB_Should_Be_Second_Name", actual.Apprenticeships.ElementAt(1).ApprenticeFirstName);
            Assert.AreEqual("CC_Should_Be_Third_Name", actual.Apprenticeships.ElementAt(2).ApprenticeFirstName);
        }

        [Test, MoqAutoData]
        public async Task Then_Approved_Apprentices_Are_Sorted_By_Uln(
            GetApprovedApprenticesRequest request,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var approvedApprenticeships = new List<ApprovedApprenticeship>
            {
                new ApprovedApprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "BB_Should_Be_Second_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new ApprovedApprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "CC_Should_Be_Third_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new ApprovedApprenticeship
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
            approvedApprenticeships[0].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            var handler = new GetApprovedApprenticesHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Uln", actual.Apprenticeships.ElementAt(0).Uln);
            Assert.AreEqual("BB_Should_Be_Second_Uln", actual.Apprenticeships.ElementAt(1).Uln);
            Assert.AreEqual("CC_Should_Be_Third_Uln", actual.Apprenticeships.ElementAt(2).Uln);
        }

        [Test, MoqAutoData]
        public async Task Then_Approved_Apprentices_Are_Sorted_By_Employer_Name(
            GetApprovedApprenticesRequest request,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var approvedApprenticeships = new List<ApprovedApprenticeship>
            {
                new ApprovedApprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "BB_Should_Be_Second_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new ApprovedApprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "CC_Should_Be_Third_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new ApprovedApprenticeship
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
            approvedApprenticeships[0].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            var handler = new GetApprovedApprenticesHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Employer", actual.Apprenticeships.ElementAt(0).EmployerName);
            Assert.AreEqual("BB_Should_Be_Second_Employer", actual.Apprenticeships.ElementAt(1).EmployerName);
            Assert.AreEqual("CC_Should_Be_Third_Employer", actual.Apprenticeships.ElementAt(2).EmployerName);
        }

        [Test, MoqAutoData]
        public async Task Then_Approved_Apprentices_Are_Sorted_By_Course_Name(
            GetApprovedApprenticesRequest request,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var approvedApprenticeships = new List<ApprovedApprenticeship>
            {
                new ApprovedApprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "BB_Should_Be_Second_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new ApprovedApprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "CC_Should_Be_Third_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new ApprovedApprenticeship
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
            approvedApprenticeships[0].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            var handler = new GetApprovedApprenticesHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Course", actual.Apprenticeships.ElementAt(0).CourseName);
            Assert.AreEqual("BB_Should_Be_Second_Course", actual.Apprenticeships.ElementAt(1).CourseName);
            Assert.AreEqual("CC_Should_Be_Third_Course", actual.Apprenticeships.ElementAt(2).CourseName);
        }

        [Test, MoqAutoData]
        public async Task Then_Approved_Apprentices_Are_Sorted_By_Planned_Start_Date(
            GetApprovedApprenticesRequest request,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var approvedApprenticeships = new List<ApprovedApprenticeship>
            {
                new ApprovedApprenticeship
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
                new ApprovedApprenticeship
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
                new ApprovedApprenticeship
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
            approvedApprenticeships[0].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[2].Cohort.ProviderId = request.ProviderId;

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            var handler = new GetApprovedApprenticesHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).ApprenticeLastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).ApprenticeLastName);
        }
    }
}