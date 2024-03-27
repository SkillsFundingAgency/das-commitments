using AutoFixture.NUnit3;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.SearchTests;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipPerformanceTests : SearchParameterServiceTestBase
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Will_Not_Search_For_Apprenticeships_That_Will_Not_Be_Used_On_Current_Page(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            ApprenticeshipSearchService service)
        {
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 3;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            await service.Find(searchParameters);

            mockContext.Verify(context => context.Apprenticeships, Times.Exactly(4));

            mockMapper.Verify(x => x.Map(It.Is<Apprenticeship>(app => !app.DataLockStatus.Any())), Times.Never);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Will_Not_Search_For_Apprenticeships_That_Will_Be_Skipped(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            ApprenticeshipSearchService service)
        {
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 3;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            AssignProviderToApprenticeships(searchParameters.ProviderId ?? 0, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            await service.Find(searchParameters);

            mockContext.Verify(context => context.Apprenticeships, Times.Exactly(4));

            mockMapper.Verify(x => x.Map(It.Is<Apprenticeship>(app => app.DataLockStatus.Any())), Times.Never);
        }
    }
}