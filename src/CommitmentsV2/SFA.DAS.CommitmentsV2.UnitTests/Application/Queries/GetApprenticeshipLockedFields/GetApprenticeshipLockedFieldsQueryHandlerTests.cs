using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipLockedFields;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipLockedFields;

[TestFixture]
public class GetApprenticeshipLockedFieldsQueryHandlerTests  
{
    private GetApprenticeshipLockedFieldsQueryHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new GetApprenticeshipLockedFieldsQueryHandlerTestsFixture();
    }

    [Test]
    public async Task Handle_WhenApprenticeshipNotFound_ThenReturnsEmptyLockedManualFields()
    {
        var result = await _fixture.Handle();

        result.Should().BeNull();
    }

    [Test]
    public async Task Handle_WhenProviderIsInUkprnList_ThenReturnsLockedManualFieldsFromConfiguration()
    {
        var expectedLockedManualFields = new List<string> { "StartDate", "Cost" };

        _fixture
            .SeedApprenticeship(providerId: 12345)
            .WithConfiguration(new CocApprovalsConfiguration
            {
                UkPrns = new List<long> { 12345, 99999 },
                LockedManualFields = expectedLockedManualFields
            });

        var result = await _fixture.Handle();

        result.LockedManualFields.Should().BeEquivalentTo(expectedLockedManualFields, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task Handle_WhenProviderIsNotInUkprnList_ThenReturnsEmptyLockedManualFields()
    {
        _fixture
            .SeedApprenticeship(providerId: 12345)
            .WithConfiguration(new CocApprovalsConfiguration
            {
                UkPrns = new List<long> { 54321 },
                LockedManualFields = new List<string> { "StartDate", "Cost" }
            });

        var result = await _fixture.Handle();

        result.LockedManualFields.Should().BeEmpty();
    }

    public class GetApprenticeshipLockedFieldsQueryHandlerTestsFixture
    {
        private readonly Mock<ProviderCommitmentsDbContext> _db;
        private CocApprovalsConfiguration _configuration;
        private GetApprenticeshipLockedFieldsQuery _request;

        public GetApprenticeshipLockedFieldsQueryHandlerTestsFixture()
        {
            _db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
                { CallBase = true };

            _request = new GetApprenticeshipLockedFieldsQuery(1);
        }

        public GetApprenticeshipLockedFieldsQueryHandlerTestsFixture SeedApprenticeship(long apprenticeshipId = 1, long providerId = 12345)
        {
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,
                Cohort = new Cohort
                {
                    ProviderId = providerId
                }
            };

            _db.Setup(context => context.Apprenticeships)
                .ReturnsDbSet(new List<Apprenticeship> { apprenticeship });

            _request = new GetApprenticeshipLockedFieldsQuery(apprenticeshipId);

            return this;
        }

        public GetApprenticeshipLockedFieldsQueryHandlerTestsFixture WithConfiguration(CocApprovalsConfiguration configuration)
        {
            _configuration = configuration;
            return this;
        }

        public async Task<GetApprenticeshipLockedFieldsQueryResult> Handle()
        {
            var handler = new GetApprenticeshipLockedFieldsQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db.Object), _configuration);
            return await handler.Handle(_request, CancellationToken.None);
        }
    }
}
