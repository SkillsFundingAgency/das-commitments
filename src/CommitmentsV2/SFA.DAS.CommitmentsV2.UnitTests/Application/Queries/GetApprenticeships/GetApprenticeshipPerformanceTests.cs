using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.GetApprenticeshipsHandlerTests;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipPerformanceTests : GetApprenticeshipsHandlerTestBase
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Will_Not_Search_For_Apprenticeships_That_Will_Not_Be_Used_On_Current_Page(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = null;
            query.PageNumber = 0;
            query.PageItemCount = 3;
            query.ReverseSort = false;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(query);

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            await handler.Handle(query, CancellationToken.None);

            mockContext.Verify(context => context.Apprenticeships, Times.Exactly(4));

            mockMapper.Verify(x => x.Map(It.Is<Apprenticeship>(app => !app.DataLockStatus.Any())), Times.Never);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Will_Not_Search_For_Apprenticeships_That_Will_Be_Skipped(
            GetApprenticeshipsQuery query,
            [Frozen] Mock<ICommitmentsReadOnlyDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = null;
            query.PageNumber = 2;
            query.PageItemCount = 3;
            query.ReverseSort = false;
            query.SearchFilters = new ApprenticeshipSearchFilters();

            var apprenticeships = GetTestApprenticeshipsWithAlerts(query);

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            await handler.Handle(query, CancellationToken.None);

            mockContext.Verify(context => context.Apprenticeships, Times.Exactly(4));

            mockMapper.Verify(x => x.Map(It.Is<Apprenticeship>(app => app.DataLockStatus.Any())), Times.Never);
        }
    }
}