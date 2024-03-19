using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.GetApprenticeshipsHandlerTests
{
    public class WhenGettingEmployerApprenticeships
    {
        [Test, RecursiveMoqAutoData]
        public async Task ThenQueriesApprenticeshipsWithEmployerIdWhenNotOrdering(
            List<Apprenticeship> apprenticeships,
            [Frozen]GetApprenticeshipsQuery query,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "";
            query.ProviderId = null;

            apprenticeships[1].Cohort.EmployerAccountId = query.EmployerAccountId.Value;

            mockSearch.Setup(x => x.Find(It.IsAny<ApprenticeshipSearchParameters>()))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = apprenticeships
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship>());

            await handler.Handle(query, CancellationToken.None);

            mockSearch.Verify(x => x.Find(It.Is<ApprenticeshipSearchParameters>(sp => 
                sp.EmployerAccountId.Equals(query.EmployerAccountId) &&
                sp.ProviderId == null)), Times.Once);
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenQueriesApprenticeshipsWithEmployerIdWhenOrdering(
            
            [Frozen]GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "test";
            query.ReverseSort = false;
            query.ProviderId = null;

            apprenticeships[1].Cohort.EmployerAccountId = query.EmployerAccountId.Value;

            mockSearch.Setup(x => x.Find(It.IsAny<OrderedApprenticeshipSearchParameters>()))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = apprenticeships
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship>());

            await handler.Handle(query, CancellationToken.None);

            mockSearch.Verify(x => x.Find(It.Is<OrderedApprenticeshipSearchParameters>(sp => 
                sp.EmployerAccountId.Equals(query.EmployerAccountId) &&
                sp.ProviderId == null)), Times.Once);
        }


        [Test, RecursiveMoqAutoData]
        public async Task ThenQueriesApprenticeshipsWithEmployerIdWhenOrderingInReverse(
           
            [Frozen]GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "test";
            query.ReverseSort = true;
            query.ProviderId = null;

            apprenticeships[1].Cohort.EmployerAccountId = query.EmployerAccountId.Value;

            mockSearch.Setup(x => x.Find(It.IsAny<ReverseOrderedApprenticeshipSearchParameters>()))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = apprenticeships
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship>());

            await handler.Handle(query, CancellationToken.None);

            mockSearch.Verify(x => x.Find(It.Is<ReverseOrderedApprenticeshipSearchParameters>(sp => 
                sp.EmployerAccountId.Equals(query.EmployerAccountId) &&
                sp.ProviderId == null)), Times.Once);
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenWillReturnCurrentSelectedPage(
            [Frozen]GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            ApprenticeshipSearchResult searchResult,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "test";
            query.ReverseSort = true;
            query.ProviderId = null;

            apprenticeships[1].Cohort.EmployerAccountId = query.EmployerAccountId.Value;

            mockSearch.Setup(x => x.Find(It.IsAny<ReverseOrderedApprenticeshipSearchParameters>()))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = apprenticeships
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship>());

            mockSearch.Setup(x => x.Find(It.IsAny<ReverseOrderedApprenticeshipSearchParameters>()))
                .ReturnsAsync(searchResult);

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.That(result.PageNumber, Is.EqualTo(searchResult.PageNumber));
        }
    }
}
