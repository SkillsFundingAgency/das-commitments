using SFA.DAS.CommitmentsV2.Application.Queries.GetInvalidIlrChanges;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetInvalidIlrChanges;

[TestFixture]
public class GetInvalidIlrChangesQueryHandlerTests
{
    private GetInvalidIlrChangesQueryHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new GetInvalidIlrChangesQueryHandlerTestsFixture();
    }

    [TearDown]
    public void TearDown()
    {
        _fixture?.Dispose();
    }

    [Test]
    public async Task Handle_ThenReturnsUnacknowledgedAutoRejectedSetsForTheProvider()
    {
        var result = await _fixture.Handle();

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Doe");
        result.RequestSets.Should().HaveCount(1);
        result.RequestSets[0].ApprovalRequestId.Should().Be(_fixture.UnacknowledgedRequestId);
        result.RequestSets[0].Decision.Should().Be("Auto rejected");
        result.RequestSets[0].Fields.Should().ContainSingle(field =>
            field.Field == "TNP1" &&
            field.Old == "1000" &&
            field.New == "0" &&
            field.Reason == "Price is zero");
    }

    [Test]
    public async Task Handle_ThenDoesNotReturnEmployerRejectedSetsForAutoRejectedQuery()
    {
        var result = await _fixture.Handle();

        result.RequestSets.Should().NotContain(set => set.ApprovalRequestId == _fixture.EmployerRejectedRequestId);
    }

    [Test]
    public async Task Handle_ThenReturnsUnacknowledgedEmployerRejectedSetsWhenRequested()
    {
        var result = await _fixture.Handle(
            itemStatus: CocApprovalItemStatus.EmployerRejected,
            decision: GetInvalidIlrChangesQuery.EmployerRejectedDecision);

        result.RequestSets.Should().ContainSingle(set =>
            set.ApprovalRequestId == _fixture.EmployerRejectedRequestId &&
            set.Decision == "Declined" &&
            set.Fields.Any(field => field.Field == "TNP1" && field.Old == "7268" && field.New == "8268"));
        result.RequestSets.Should().NotContain(set => set.ApprovalRequestId == _fixture.UnacknowledgedRequestId);
    }

    [Test]
    public async Task Handle_ThenDoesNotReturnAcknowledgedOrNonAutoRejectedSets()
    {
        var result = await _fixture.Handle();

        result.RequestSets.Should().NotContain(set => set.ApprovalRequestId == _fixture.AcknowledgedRequestId);
        result.RequestSets.Should().NotContain(set => set.ApprovalRequestId == _fixture.PendingRequestId);
    }

    [Test]
    public async Task Handle_ThenReturnsNullWhenApprenticeshipIsMissing()
    {
        var result = await _fixture.Handle(apprenticeshipId: 999);

        result.Should().BeNull();
    }

    [Test]
    public async Task Handle_ThenThrowsWhenProviderDoesNotOwnTheApprenticeship()
    {
        var act = () => _fixture.Handle(providerId: 999);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class GetInvalidIlrChangesQueryHandlerTestsFixture : IDisposable
    {
        private readonly GetInvalidIlrChangesQueryHandler _handler;
        private readonly ProviderCommitmentsDbContext _db;

        public Guid UnacknowledgedRequestId { get; } = Guid.NewGuid();
        public Guid AcknowledgedRequestId { get; } = Guid.NewGuid();
        public Guid PendingRequestId { get; } = Guid.NewGuid();
        public Guid EmployerRejectedRequestId { get; } = Guid.NewGuid();

        public GetInvalidIlrChangesQueryHandlerTestsFixture()
        {
            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            SeedData();
            _handler = new GetInvalidIlrChangesQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
        }

        public Task<Api.Types.Responses.GetInvalidIlrChangesResponse> Handle(
            long apprenticeshipId = 1001,
            long providerId = 333,
            CocApprovalItemStatus itemStatus = CocApprovalItemStatus.AutoRejected,
            string decision = GetInvalidIlrChangesQuery.AutoRejectedDecision)
        {
            return _handler.Handle(new GetInvalidIlrChangesQuery
            {
                ApprenticeshipId = apprenticeshipId,
                ProviderId = providerId,
                ItemStatus = itemStatus,
                Decision = decision
            }, CancellationToken.None);
        }

        private void SeedData()
        {
            var cohort = new Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.EmployerAccountId, 222);

            var apprenticeship = new Apprenticeship
            {
                Id = 1001,
                FirstName = "Jane",
                LastName = "Doe",
                Cohort = cohort,
                PaymentStatus = PaymentStatus.Active
            };

            apprenticeship.ApprovalRequests = new List<ApprovalRequest>
            {
                CreateRequest(UnacknowledgedRequestId, CocApprovalResultStatus.Complete, null, CocApprovalItemStatus.AutoRejected, "Price is zero"),
                CreateRequest(AcknowledgedRequestId, CocApprovalResultStatus.Complete, DateTime.UtcNow, CocApprovalItemStatus.AutoRejected, "Already seen"),
                CreateRequest(PendingRequestId, CocApprovalResultStatus.Pending, null, CocApprovalItemStatus.Pending, "Waiting"),
                CreateRequest(EmployerRejectedRequestId, CocApprovalResultStatus.Complete, null, CocApprovalItemStatus.EmployerRejected, "Employer declined", "7268", "8268")
            };

            _db.Cohorts.Add(cohort);
            _db.Apprenticeships.Add(apprenticeship);
            _db.SaveChanges();
        }

        private static ApprovalRequest CreateRequest(
            Guid id,
            CocApprovalResultStatus status,
            DateTime? acknowledgedAt,
            CocApprovalItemStatus itemStatus,
            string reason,
            string oldValue = "1000",
            string newValue = "0")
        {
            return new ApprovalRequest
            {
                Id = id,
                ApprenticeshipId = 1001,
                Status = status,
                ProviderAcknowledgedAt = acknowledgedAt,
                ProviderAcknowledgedBy = acknowledgedAt == null ? null : "user-1",
                Reason = reason,
                Items = new List<ApprovalFieldRequest>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Field = "TNP1",
                        Old = oldValue,
                        New = newValue,
                        EffectiveFromDate = new DateTime(2026, 8, 1),
                        Status = itemStatus,
                        Reason = reason
                    }
                }
            };
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
