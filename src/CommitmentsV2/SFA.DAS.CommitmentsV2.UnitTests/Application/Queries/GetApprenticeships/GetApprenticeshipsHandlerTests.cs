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
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;
//using ApprenticeshipUpdateStatus = SFA.DAS.CommitmentsV2.Models.ApprenticeshipUpdateStatus;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsHandlerTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Apprenticeships(
            GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.PageNumber = 0;
            query.PageItemCount = 0;

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

            result.Apprenticeships.Count()
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId));
            result.Apprenticeships.Should().AllBeEquivalentTo(apprenticeshipDetails);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Apprenticeships_Total_Found(
            GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.PageNumber = 0;
            query.PageItemCount = 0;

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

            result.TotalApprenticeshipsFound
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId));
        }

        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_Then_Apprentices_Are_Default_Sorted(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = "";
            query.ReverseSort = false;
            query.PageNumber = 0;
            query.PageItemCount = 0;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "XX",
                    Uln = "Should_Be_Third",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).Uln);
            Assert.AreEqual("Should_Be_Fourth", actual.Apprenticeships.ElementAt(3).EmployerName);
            Assert.AreEqual("Should_Be_Fifth", actual.Apprenticeships.ElementAt(4).CourseName);
            Assert.AreEqual(DateTime.UtcNow.AddMonths(2).ToString("d"), actual.Apprenticeships.ElementAt(5).StartDate.ToString("d"));
        }

        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_And_Is_Reverse_Sorted_Then_Apprentices_Are_Default_Sorted_In_Reverse(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = null;
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Second",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>{new DataLockStatus { IsResolved = false, Status = Status.Fail, EventStatus = 1}}
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_First",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
        }


        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_And_Is_And_There_Are_ApprenticeshipUpdates_These_Appear_First(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = null;
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.ReverseSort = false;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Second",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
                    {
                        new ApprenticeshipUpdate
                        {
                            Status = ApprenticeshipUpdateStatus.Pending,
                            Originator = Originator.Employer
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
                    {
                        new ApprenticeshipUpdate
                        {
                            Status = ApprenticeshipUpdateStatus.Pending,
                            Originator = Originator.Provider
                        }
                    }
                },
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(2).LastName);

        }

        [Test, MoqAutoData]
        public async Task And_Sorted_By_Alerts_And_Is_Reverse_Sorted_Then_Apprentices_Are_Default_Sorted(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = "DataLockStatus";
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Second",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>(),
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>{new ApprenticeshipUpdate
                    {
                        Status = (byte)ApprenticeshipUpdateStatus.Pending,
                        Originator = (byte)Originator.Employer
                    }},
                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_First",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    DataLockStatus = new List<DataLockStatus>{new DataLockStatus
                        {
                            IsResolved = false,
                            Status = Status.Fail,
                            EventStatus = 2
                        }
                    },
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);

        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Return_Per_Page(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = null;
            query.PageNumber = 2;
            query.PageItemCount = 2;
            query.ReverseSort = false;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

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
        public async Task Then_Apprentices_Total_Found_Are_Return_With_Page_Data(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = null;
            query.PageNumber = 2;
            query.PageItemCount = 2;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(apprenticeships.Count, actual.TotalApprenticeshipsFound);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_With_Alerts_Total_Found_Are_Return_With_Page_Data(
           GetApprenticeshipsQuery query,
           [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = null;
            query.PageNumber = 2;
            query.PageItemCount = 2;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.TotalApprenticeshipsWithAlertsFound);
        }

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

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

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
        public async Task Then_Reversed_Ordered_Apprentices_Are_Return_Per_Page(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = null;
            query.PageNumber = 1;
            query.PageItemCount = 2;
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);


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
        public async Task Then_Reverse_Sorted_Apprentices_Are_Return_Per_Page(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            query.SortField = nameof(Apprenticeship.FirstName);
            query.PageNumber = 2;
            query.PageItemCount = 2;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.Apprenticeships.Count());
            Assert.AreEqual("B", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("A", actual.Apprenticeships.ElementAt(1).FirstName);
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

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.TotalApprenticeshipsWithAlertsFound);
        }

        [Test, MoqAutoData]
        public async Task Then_Reversed_Ordered_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = null;
            query.PageNumber = 2;
            query.PageItemCount = 2;
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.TotalApprenticeshipsWithAlertsFound);
        }

        [Test, MoqAutoData]
        public async Task Then_Reverse_Sorted_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            query.SortField = nameof(Apprenticeship.FirstName);
            query.PageNumber = 2;
            query.PageItemCount = 2;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.TotalApprenticeshipsWithAlertsFound);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Not_Returned_If_Page_Count_Is_Greater_Than_Items_Available(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = GetTestApprenticeships(query);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            query.SortField = null;
            query.PageNumber = 5;
            query.PageItemCount = 2;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.IsEmpty(actual.Apprenticeships);

        }


        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "BB_Should_Be_Second_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
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
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SortField = nameof(Apprenticeship.FirstName);
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "BB_Should_Be_Second_Name",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);


            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("CC_Should_Be_First_Name", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("BB_Should_Be_Second_Name", actual.Apprenticeships.ElementAt(1).FirstName);
            Assert.AreEqual("AA_Should_Be_Third_Name", actual.Apprenticeships.ElementAt(2).FirstName);
            Assert.AreEqual("Zog", actual.Apprenticeships.ElementAt(2).LastName);
            Assert.AreEqual("AA_Should_Be_Third_Name", actual.Apprenticeships.ElementAt(3).FirstName);
            Assert.AreEqual("Fog", actual.Apprenticeships.ElementAt(3).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Uln(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "BB_Should_Be_Second_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "CC_Should_Be_Third_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "AA_Should_Be_First_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
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
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Uln(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SortField = nameof(Apprenticeship.Uln);
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "BB_Should_Be_Second_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "CC_Should_Be_First_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "AA_Should_Be_Third_Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("CC_Should_Be_First_Uln", actual.Apprenticeships.ElementAt(0).Uln);
            Assert.AreEqual("BB_Should_Be_Second_Uln", actual.Apprenticeships.ElementAt(1).Uln);
            Assert.AreEqual("AA_Should_Be_Third_Uln", actual.Apprenticeships.ElementAt(2).Uln);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Employer_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "BB_Should_Be_Second_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "CC_Should_Be_Third_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "AA_Should_Be_First_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            query.SortField = nameof(Apprenticeship.Cohort.LegalEntityName);
            query.ReverseSort = false;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First_Employer", actual.Apprenticeships.ElementAt(0).EmployerName);
            Assert.AreEqual("BB_Should_Be_Second_Employer", actual.Apprenticeships.ElementAt(1).EmployerName);
            Assert.AreEqual("CC_Should_Be_Third_Employer", actual.Apprenticeships.ElementAt(2).EmployerName);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Employer_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SortField = nameof(Apprenticeship.Cohort.LegalEntityName);
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "BB_Should_Be_Second_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "CC_Should_Be_First_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "AA_Should_Be_Third_Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("CC_Should_Be_First_Employer", actual.Apprenticeships.ElementAt(0).EmployerName);
            Assert.AreEqual("BB_Should_Be_Second_Employer", actual.Apprenticeships.ElementAt(1).EmployerName);
            Assert.AreEqual("AA_Should_Be_Third_Employer", actual.Apprenticeships.ElementAt(2).EmployerName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Course_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "BB_Should_Be_Second_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "CC_Should_Be_Third_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "AA_Should_Be_First_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
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
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Course_Name(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SortField = nameof(Apprenticeship.CourseName);
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "BB_Should_Be_Second_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "CC_Should_Be_First_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    Uln = "Uln",
                    CourseName = "AA_Should_Be_Third_Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("CC_Should_Be_First_Course", actual.Apprenticeships.ElementAt(0).CourseName);
            Assert.AreEqual("BB_Should_Be_Second_Course", actual.Apprenticeships.ElementAt(1).CourseName);
            Assert.AreEqual("AA_Should_Be_Third_Course", actual.Apprenticeships.ElementAt(2).CourseName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Planned_Start_Date(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
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
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Planned_Start_Date(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SortField = nameof(Apprenticeship.StartDate);
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "BB_Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);


            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("AA_Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("BB_Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("CC_Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Planned_End_Date(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
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
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Planned_End_Date(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SortField = nameof(Apprenticeship.EndDate);
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Apprentices_Are_Sorted_By_Payment_Status(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Active
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);
            query.SortField = nameof(Apprenticeship.PaymentStatus);
            query.ReverseSort = false;

            //Act
            var actual = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
            Assert.AreEqual("Should_Be_Third", actual.Apprenticeships.ElementAt(2).LastName);
        }

        [Test, MoqAutoData]
        public async Task And_Is_Reverse_Sorted_Then_Apprentices_Are_Sorted_By_Payment_Status(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.PageNumber = 0;
            query.PageItemCount = 0;
            query.SortField = nameof(Apprenticeship.PaymentStatus);
            query.ReverseSort = true;

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
                    DataLockStatus = new List<DataLockStatus>(),
                    PaymentStatus = PaymentStatus.Active
                    }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsQueryHandler(mockContext.Object, mapper);

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

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper();
            var Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "FirstName",
                    LastName = "Should_Be_Second",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "Employer"},
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

        private static List<Apprenticeship> GetTestApprenticeships(GetApprenticeshipsQuery query)
        {
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "A",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>{new DataLockStatus { IsResolved = false, Status = Status.Fail, EventStatus = 1} }
                },
                new Apprenticeship
                {
                    FirstName = "B",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>{new DataLockStatus { IsResolved = false, Status = Status.Fail, EventStatus = 1} }
                },
                new Apprenticeship
                {
                    FirstName = "C",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "D",
                    LastName = "Fog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            return apprenticeships;
        }

        private static void AssignProviderToApprenticeships(long providerId, IEnumerable<Apprenticeship> apprenticeships)
        {
            foreach (var apprenticeship in apprenticeships)
            {
                apprenticeship.Cohort.ProviderId = providerId;
            }
        }
    }
}