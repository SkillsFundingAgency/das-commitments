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
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;
using ApprenticeshipUpdateStatus = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.ApprenticeshipUpdateStatus;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.GetApprenticeshipsHandlerTests
{
    public class WhenGettingUnsortedApprenticeships : GetApprenticeshipsHandlerTestBase
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Apprenticeships(
            GetApprenticeshipsQuery request,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SearchFilters = new ApprenticeshipSearchFilters();

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

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Apprenticeships_Total_Found(
            GetApprenticeshipsQuery request,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SearchFilters = new ApprenticeshipSearchFilters();

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

            result.TotalApprenticeshipsFound
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId));
        }

        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_Then_Apprentices_Are_Default_Sorted(
            GetApprenticeshipsQuery request,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = "";
            request.ReverseSort = false;
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());
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
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
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
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Should_Be_Fourth"},
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
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
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
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
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
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                }
            };

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).Uln);
            Assert.AreEqual("Should_Be_Fourth", actual.Apprenticeships.ElementAt(3).EmployerName);
            Assert.AreEqual("Should_Be_Fifth", actual.Apprenticeships.ElementAt(4).CourseName);
            Assert.AreEqual(DateTime.UtcNow.AddMonths(2).ToString("d"), actual.Apprenticeships.ElementAt(5).StartDate.ToString("d"));
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Return_Per_Page(
            GetApprenticeshipsQuery request,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = null;
            request.PageNumber = 2;
            request.PageItemCount = 2;
            request.ReverseSort = false;
            request.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = GetTestApprenticeships(request);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.Apprenticeships.Count());
            Assert.AreEqual("C", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("D", actual.Apprenticeships.ElementAt(1).FirstName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Total_Found_Are_Return_With_Page_Data(
            GetApprenticeshipsQuery request,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = null;
            request.PageNumber = 2;
            request.PageItemCount = 2;
            request.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = GetTestApprenticeships(request);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual(apprenticeships.Count, actual.TotalApprenticeshipsFound);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            GetApprenticeshipsQuery request,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = null;
            request.PageNumber = 2;
            request.PageItemCount = 2;
            request.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = GetTestApprenticeships(request);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.TotalApprenticeshipsWithAlertsFound);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Not_Returned_If_Page_Count_Is_Greater_Than_Items_Available(
            GetApprenticeshipsQuery query,
          
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            var apprenticeships = GetTestApprenticeships(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            query.SortField = null;
            query.PageNumber = 5;
            query.PageItemCount = 2;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.IsEmpty(actual.Apprenticeships);

        }

        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_And_Is_And_There_Are_ApprenticeshipUpdates_These_Appear_First(
            GetApprenticeshipsQuery request,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = null;
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.ReverseSort = false;
            request.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());
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
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
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
                    ProviderRef = request.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
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

            AssignProviderToApprenticeships(request.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(2).LastName);
            
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Total_Available_Apprenticeships(
            GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            var filterCourseName = "Test Corse";
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SearchFilters = new ApprenticeshipSearchFilters
            {
                CourseName = filterCourseName
            };

            apprenticeships[0].CourseName = filterCourseName;

            apprenticeships[0].Cohort.ProviderId = query.ProviderId;
            apprenticeships[1].Cohort.ProviderId = query.ProviderId;


            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            mockMapper
                .Setup(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId))))
                .ReturnsAsync(apprenticeshipDetails);

            var result = await handler.Handle(query, CancellationToken.None);

            result.TotalApprenticeshipsFound.Should().Be(1);

            result.TotalApprenticeships
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId));
        }

    }
}
