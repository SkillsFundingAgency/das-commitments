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
    public class WhenGettingUnsortedApprenticeshipsInReverserOrder : GetApprenticeshipsHandlerTestBase
    {
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
            query.SearchFilters = new ApprenticeshipSearchFilters();

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
                    EndDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>{new DataLockStatus { IsResolved = false, Status = Status.Fail, EventStatus = EventStatus.New}}

                },
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_First",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
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
        public async Task Then_Reversed_Ordered_Apprentices_Are_Return_Per_Page(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            query.SortField = null;
            query.PageNumber = 1;
            query.PageItemCount = 2;
            query.ReverseSort = true;
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
            Assert.AreEqual("D", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("E", actual.Apprenticeships.ElementAt(1).FirstName);
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
    }
}
