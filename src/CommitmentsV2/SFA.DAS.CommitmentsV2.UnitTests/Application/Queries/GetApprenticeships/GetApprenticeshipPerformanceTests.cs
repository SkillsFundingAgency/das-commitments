using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Handlers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.SearchTests;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipPerformanceTests : SearchParameterHandlerTestBase
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Will_Not_Search_For_Apprenticeships_That_Will_Not_Be_Used_On_Current_Page(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            ApprenticeshipSearchHandler handler)
        {
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 3;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            await handler.Find(searchParameters);

            mockContext.Verify(context => context.Apprenticeships, Times.Exactly(3));

            mockMapper.Verify(x => x.Map(It.Is<Apprenticeship>(app => !app.DataLockStatus.Any())), Times.Never);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Will_Not_Search_For_Apprenticeships_That_Will_Be_Skipped(
            ApprenticeshipSearchParameters searchParameters,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            ApprenticeshipSearchHandler handler)
        {
            searchParameters.PageNumber = 2;
            searchParameters.PageItemCount = 3;
            searchParameters.ReverseSort = false;
            searchParameters.Filters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(searchParameters);

            AssignProviderToApprenticeships(searchParameters.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            await handler.Find(searchParameters);

            mockContext.Verify(context => context.Apprenticeships, Times.Exactly(3));

            mockMapper.Verify(x => x.Map(It.Is<Apprenticeship>(app => app.DataLockStatus.Any())), Times.Never);
        }
    }
}
