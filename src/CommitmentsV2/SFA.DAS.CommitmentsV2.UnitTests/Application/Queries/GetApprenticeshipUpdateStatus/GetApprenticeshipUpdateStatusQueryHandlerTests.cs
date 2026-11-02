using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdateStatus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipUpdateStatus;

[TestFixture]
public class GetApprenticeshipUpdateStatusQueryHandlerTests
{
    private GetApprenticeshipUpdateStatusQueryHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new GetApprenticeshipUpdateStatusQueryHandlerTestsFixture();
    }

    [Test]
    public async Task Handle_WhenApprenticeshipNotFound_ThenReturnsEmptyLockedManualFields()
    {
        var result = await _fixture.Handle();

        result.Should().NotBeNull();
        result.LockedManualFields.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_WhenProviderIsInUkprnList_ThenReturnsLockedManualFieldsFromConfiguration()
    {
        var expectedLockedManualFields = new List<string> { "StartDate", "Cost" };

        _fixture
            .SeedApprenticeship(providerId: 12345)
            .WithConfiguration(new CoCApprovalsConfiguration
            {
                UKPRNS = new List<long> { 12345, 99999 },
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
            .WithConfiguration(new CoCApprovalsConfiguration
            {
                UKPRNS = new List<long> { 54321 },
                LockedManualFields = new List<string> { "StartDate", "Cost" }
            });

        var result = await _fixture.Handle();

        result.LockedManualFields.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_WhenConfigurationIsNull_ThenReturnsEmptyLockedManualFields()
    {
        _fixture
            .SeedApprenticeship(providerId: 12345)
            .WithConfiguration(null);

        var result = await _fixture.Handle();

        result.LockedManualFields.Should().BeEmpty();
    }

    public class GetApprenticeshipUpdateStatusQueryHandlerTestsFixture
    {
        private readonly Mock<ProviderCommitmentsDbContext> _db;
        private CoCApprovalsConfiguration _configuration;
        private GetApprenticeshipUpdateStatusQuery _request;

        public GetApprenticeshipUpdateStatusQueryHandlerTestsFixture()
        {
            _db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
                { CallBase = true };

            _request = new GetApprenticeshipUpdateStatusQuery(1);
        }

        public GetApprenticeshipUpdateStatusQueryHandlerTestsFixture SeedApprenticeship(long apprenticeshipId = 1, long providerId = 12345)
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

            _request = new GetApprenticeshipUpdateStatusQuery(apprenticeshipId);

            return this;
        }

        public GetApprenticeshipUpdateStatusQueryHandlerTestsFixture WithConfiguration(CoCApprovalsConfiguration configuration)
        {
            _configuration = configuration;
            return this;
        }

        public async Task<GetApprenticeshipUpdateStatusQueryResult> Handle()
        {
            var handler = new GetApprenticeshipUpdateStatusQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db.Object), _configuration);
            return await handler.Handle(_request, CancellationToken.None);
        }
    }
}
