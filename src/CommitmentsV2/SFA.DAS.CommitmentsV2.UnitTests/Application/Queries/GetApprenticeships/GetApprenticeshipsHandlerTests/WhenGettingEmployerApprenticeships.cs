using AutoFixture.NUnit3;
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
            [Frozen] GetApprenticeshipsQuery query,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            Mock<ProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper)
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

            mockContext.Setup(t => t.LearningChangeHistory).ReturnsDbSet([]);

            var lazyContext = new Lazy<ProviderCommitmentsDbContext>(() => mockContext.Object);

            GetApprenticeshipsQueryHandler handler = new(mockMapper.Object, mockSearch.Object, lazyContext);

            await handler.Handle(query, CancellationToken.None);

            mockSearch.Verify(x => x.Find(It.Is<ApprenticeshipSearchParameters>(sp =>
                sp.EmployerAccountId.Equals(query.EmployerAccountId) &&
                sp.ProviderId == null)), Times.Once);
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenQueriesApprenticeshipsWithEmployerIdWhenOrdering(
            [Frozen] GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            Mock<ProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper)
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

            mockContext.Setup(t => t.LearningChangeHistory).ReturnsDbSet([]);

            var lazyContext = new Lazy<ProviderCommitmentsDbContext>(() => mockContext.Object);

            GetApprenticeshipsQueryHandler handler = new(mockMapper.Object, mockSearch.Object, lazyContext);

            await handler.Handle(query, CancellationToken.None);

            mockSearch.Verify(x => x.Find(It.Is<OrderedApprenticeshipSearchParameters>(sp =>
                sp.EmployerAccountId.Equals(query.EmployerAccountId) &&
                sp.ProviderId == null)), Times.Once);
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenQueriesApprenticeshipsWithEmployerIdWhenOrderingInReverse(
            [Frozen] GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
            Mock<ProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper)
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

            mockContext.Setup(t => t.LearningChangeHistory).ReturnsDbSet([]);

            var lazyContext = new Lazy<ProviderCommitmentsDbContext>(() => mockContext.Object);

            GetApprenticeshipsQueryHandler handler = new(mockMapper.Object, mockSearch.Object, lazyContext);

            await handler.Handle(query, CancellationToken.None);

            mockSearch.Verify(x => x.Find(It.Is<ReverseOrderedApprenticeshipSearchParameters>(sp =>
                sp.EmployerAccountId.Equals(query.EmployerAccountId) &&
                sp.ProviderId == null)), Times.Once);
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenWillReturnCurrentSelectedPage(
            [Frozen] GetApprenticeshipsQuery query,
            List<Apprenticeship> apprenticeships,
            ApprenticeshipSearchResult searchResult,
            Mock<ProviderCommitmentsDbContext> mockContext,
            List<LearningChangeHistory> changeHistory,
            [Frozen] Mock<IApprenticeshipSearch> mockSearch,
            [Frozen] Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper)
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
            mockContext.Setup(t => t.LearningChangeHistory).ReturnsDbSet([]);

            var lazyContext = new Lazy<ProviderCommitmentsDbContext>(() => mockContext.Object);

            GetApprenticeshipsQueryHandler handler = new(mockMapper.Object, mockSearch.Object, lazyContext);

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.That(result.PageNumber, Is.EqualTo(searchResult.PageNumber));
        }

        [Test, RecursiveMoqAutoData]
        public async Task ThenReturnsApprenticeshipsData(
        List<Apprenticeship> apprenticeships,
        GetApprenticeshipsQueryResult.ApprenticeshipDetails apprenticeshipDetails,
        [Frozen] GetApprenticeshipsQuery query,
        [Frozen] Mock<IApprenticeshipSearch> search,
        Mock<ProviderCommitmentsDbContext> mockContext,
        List<LearningChangeHistory> changeHistory,
        Mock<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>> mockMapper)
        {
            query.SortField = "";
            query.EmployerAccountId = null;

            apprenticeships[1].Cohort.ProviderId = query.ProviderId ?? 0;

            search.Setup(x => x.Find(It.IsAny<ApprenticeshipSearchParameters>()))
                .ReturnsAsync(new ApprenticeshipSearchResult
                {
                    Apprenticeships = Array.Empty<Apprenticeship>(),
                    TotalAvailableApprenticeships = apprenticeships.Count(),
                });

            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship>());

            mockMapper
                .Setup(mapper => mapper.Map(It.IsIn(apprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId))))
                .ReturnsAsync(apprenticeshipDetails);

            mockContext.Setup(t => t.LearningChangeHistory).ReturnsDbSet(changeHistory);

            var lazyContext = new Lazy<ProviderCommitmentsDbContext>(() => mockContext.Object);

            GetApprenticeshipsQueryHandler handler = new(mockMapper.Object, search.Object, lazyContext);

            var result = await handler.Handle(query, CancellationToken.None);

            result.TotalApprenticeships.Should().Be(apprenticeships.Count);
        }
    }
}