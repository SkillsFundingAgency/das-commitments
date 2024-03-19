using System;
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
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.SearchTests
{
    public class WhenDownloadingApprentices : SearchParameterServiceTestBase
    {
        [Test, MoqAutoData]
        public async Task Then_Downloads_Are_Restricted_To_Twelve_Months_for_Default_Search(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.PageNumber = 0;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.CancellationToken = CancellationToken.None;
            searchParameters.EmployerAccountId = null;


            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            apprenticeships[1].ProviderRef = null;
            apprenticeships[1].EndDate = DateTime.UtcNow.AddMonths(-13);;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.Apprenticeships.Count(), Is.EqualTo(apprenticeships.Count - 1));
            Assert.That(actual.Apprenticeships.Contains(apprenticeships[1]), Is.False);
        }
        [Test, MoqAutoData]
        public async Task Then_Downloads_Are_Restricted_To_Twelve_Months_Normal_Sort(
            OrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.FieldName = nameof(Apprenticeship.FirstName);
            searchParameters.PageNumber = 0;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.CancellationToken = CancellationToken.None;
            searchParameters.EmployerAccountId = null;


            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            apprenticeships[1].ProviderRef = null;
            apprenticeships[1].EndDate = DateTime.UtcNow.AddMonths(-13); ;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new OrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.Apprenticeships.Count(), Is.EqualTo(apprenticeships.Count - 1));
            Assert.That(actual.Apprenticeships.Contains(apprenticeships[1]), Is.False);
        }
        [Test, MoqAutoData]
        public async Task Then_Downloads_Are_Restricted_To_Twelve_Months_For_Reverse_Sort(
            ReverseOrderedApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext)
        {
            //Arrange
            searchParameters.FieldName = nameof(Apprenticeship.FirstName);
            searchParameters.PageNumber = 0;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.CancellationToken = CancellationToken.None;
            searchParameters.EmployerAccountId = null;


            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            apprenticeships[1].ProviderRef = null;
            apprenticeships[1].EndDate = DateTime.UtcNow.AddMonths(-13); ;

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var service = new ReverseOrderedApprenticeshipSearchService(mockContext.Object);

            //Act
            var actual = await service.Find(searchParameters);

            //Assert
            Assert.That(actual.Apprenticeships.Count(), Is.EqualTo(apprenticeships.Count - 1));
            Assert.That(actual.Apprenticeships.Contains(apprenticeships[1]), Is.False);
        }
    }
}
