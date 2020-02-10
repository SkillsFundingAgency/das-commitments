using System;
using System.Collections.Generic;
using System.Linq;
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
    public class WhenGettingUnsortedApprenticeshipsInReverserOrder : SearchParameterServiceTestBase
    {
        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_And_Is_Reverse_Sorted_Then_Apprentices_Are_Default_Sorted_In_Reverse(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.ReverseSort = true;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "XX",
                    LastName = "Should_Be_Second",
                    Uln = "XX",
                    CourseName = "XX",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = searchParameters.ProviderId.ToString(),
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
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    Cohort = new Cohort{LegalEntityName = "XX"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual("Should_Be_First", actual.Apprenticeships.ElementAt(0).LastName);
            Assert.AreEqual("Should_Be_Second", actual.Apprenticeships.ElementAt(1).LastName);
        }

        [Test, MoqAutoData]
        public async Task Then_Reversed_Ordered_Apprentices_Are_Return_Per_Page(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 1;
            searchParameters.PageItemCount = 2;
            searchParameters.ReverseSort = true;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);


            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual(2, actual.Apprenticeships.Count());
            Assert.AreEqual("D", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("E", actual.Apprenticeships.ElementAt(1).FirstName);
        }

        [Test, MoqAutoData]
        public async Task Then_Reversed_Ordered_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 2;
            searchParameters.ReverseSort = true;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            
            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.AreEqual(3, actual.TotalApprenticeshipsWithAlertsFound);
        }
    }
}
