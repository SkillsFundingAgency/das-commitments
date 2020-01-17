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
    public class WhenGettingUnsortedApprenticeshipsInReverserOrder : GetApprenticeshipsHandlerTestBase
    {
        [Test, MoqAutoData]
        public async Task And_No_Sort_Term_And_Is_Reverse_Sorted_Then_Apprentices_Are_Default_Sorted(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = null;
            request.PageNumber = 0;
            request.PageItemCount = 0;
            request.ReverseSort = true;
            request.SearchFilters = new ApprenticeshipSearchFilters();

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
        public async Task Then_Reversed_Ordered_Apprentices_Are_Return_Per_Page(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = null;
            request.PageNumber = 1;
            request.PageItemCount = 2;
            request.ReverseSort = true;
            request.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);

            var apprenticeships = GetTestApprenticeships(request);


            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.Apprenticeships.Count());
            Assert.AreEqual("C", actual.Apprenticeships.ElementAt(0).FirstName);
            Assert.AreEqual("D", actual.Apprenticeships.ElementAt(1).FirstName);
        }

        [Test, MoqAutoData]
        public async Task Then_Reversed_Ordered_With_Alerts_Total_Found_Are_Return_With_Page_Data(
            GetApprenticeshipsRequest request,
            Mock<IAlertsMapper> alertsMapper,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext)
        {
            //Arrange
            request.SortField = null;
            request.PageNumber = 2;
            request.PageItemCount = 2;
            request.ReverseSort = true;
            request.SearchFilters = new ApprenticeshipSearchFilters();

            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(alertsMapper.Object);

            var apprenticeships = GetTestApprenticeships(request);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);
            var handler = new GetApprenticeshipsHandler(mockContext.Object, mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual(2, actual.TotalApprenticeshipsWithAlertsFound);
        }
    }
}
