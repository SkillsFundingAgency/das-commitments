using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.UnitTests.DatabaseMock;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.GetApprenticeshipsHandlerTests
{
    public class WhenGettingProviderApprenticeships
    {
        [Test, RecursiveMoqAutoData]
        public async Task ThenReturnUnsortedApprenticeshipsWhenNotOrdering(
            List<Apprenticeship> apprenticeships,
            [Frozen]GetApprenticeshipsQuery query,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "";
            query.EmployerAccountId = null;

            apprenticeships[1].Cohort.ProviderId = query.ProviderId ?? 0;

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
                sp.ProviderId.Equals(query.ProviderId) &&
                sp.EmployerAccountId == null)), Times.Once);
            
            mockMapper
                .Verify(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId))), Times.Once());
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenReturnsOrderedApprenticeshipsWhenOrdering(
            
            [Frozen]GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "test";
            query.ReverseSort = false;
            query.EmployerAccountId = null;

            apprenticeships[1].Cohort.ProviderId = query.ProviderId ?? 0;

            mockSearch.Setup(x => x.Find(
                    It.Is<OrderedApprenticeshipSearchParameters>(sp => 
                        sp.ProviderId.Equals(query.ProviderId)&&
                        sp.EmployerAccountId.Equals(null))))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = apprenticeships
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship>());

            await handler.Handle(query, CancellationToken.None);

            mockMapper
                .Verify(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId))), Times.Once());
        }


        [Test, RecursiveMoqAutoData]
        public async Task ThenReturnReverseOrderedApprenticeshipsWhenOrderingInReverse(
           
            [Frozen]GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "test";
            query.ReverseSort = true;
            query.EmployerAccountId = null;

            apprenticeships[1].Cohort.ProviderId = query.ProviderId ?? 0;

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
                sp.ProviderId.Equals(query.ProviderId) &&
                sp.EmployerAccountId == null)), Times.Once);
            
            mockMapper
                .Verify(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId))), Times.Once());
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenReturnsMappedApprenticeships( 
            [Frozen]GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            [Frozen]GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> search,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "";
            query.EmployerAccountId = null;

            apprenticeships[1].Cohort.ProviderId = query.ProviderId ?? 0;

            search.Setup(x => x.Find(It.IsAny<ApprenticeshipSearchParameters>()))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = apprenticeships
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            mockMapper
                .Setup(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId))))
                .ReturnsAsync(apprenticeshipDetails);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Apprenticeships.Should().AllBeEquivalentTo(apprenticeshipDetails);

        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenReturnsApprenticeshipsData(
            int totalApprenticeshipsFoundWithAlertsCount,
            int totalApprenticeshipsFoundCount,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen]GetApprenticeshipsQuery query,
            [Frozen] Mock<IApprenticeshipSearch> search,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler) 
        {
            query.SortField = "";
            query.EmployerAccountId = null;

            apprenticeships[1].Cohort.ProviderId = query.ProviderId ?? 0;

            search.Setup(x => x.Find(It.IsAny<ApprenticeshipSearchParameters>()))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = new Apprenticeship[0],
                    TotalAvailableApprenticeships = apprenticeships.Count,
                    TotalApprenticeshipsFound = totalApprenticeshipsFoundCount,
                    TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsFoundWithAlertsCount
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship>());

            mockMapper
                .Setup(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId))))
                .ReturnsAsync(apprenticeshipDetails);

            var result = await handler.Handle(query, CancellationToken.None);

            result.TotalApprenticeshipsFound.Should().Be(totalApprenticeshipsFoundCount);
            result.TotalApprenticeshipsWithAlertsFound.Should().Be(totalApprenticeshipsFoundWithAlertsCount);
            result.TotalApprenticeships.Should().Be(apprenticeships.Count);
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenReturnsPageNumber(
            [Frozen]GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            ApprenticeshipSearchResult searchResult,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> search,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "";
            query.EmployerAccountId = null;

            apprenticeships[1].Cohort.ProviderId = query.ProviderId ?? 0;

            search.Setup(x => x.Find(It.IsAny<ApprenticeshipSearchParameters>()))
                .ReturnsAsync(searchResult);

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(apprenticeships);

            var result = await handler.Handle(query, CancellationToken.None);

            result.PageNumber.Should().Be(searchResult.PageNumber);
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenIsSetToBeFromProvider(
            int totalApprenticeshipsFoundWithAlertsCount,
            int totalApprenticeshipsFoundCount,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            [Frozen]GetApprenticeshipsQuery query,
            [Frozen] Mock<IApprenticeshipSearch> search,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper,
            GetApprenticeshipsQueryHandler handler)
        {
            query.SortField = "";
            query.EmployerAccountId = null;

            apprenticeships[1].Cohort.ProviderId = query.ProviderId ?? 0;

            search.Setup(x => x.Find(It.IsAny<ApprenticeshipSearchParameters>()))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = apprenticeships,
                    TotalAvailableApprenticeships = apprenticeships.Count,
                    TotalApprenticeshipsFound = totalApprenticeshipsFoundCount,
                    TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsFoundWithAlertsCount
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship>());

            mockMapper
                .Setup(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.IsProviderSearch))))
                .ReturnsAsync(apprenticeshipDetails);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Apprenticeships.Should().AllBeEquivalentTo(apprenticeshipDetails);

        }
    }
}
