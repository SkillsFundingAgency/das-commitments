using AutoFixture.Kernel;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovalRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovalRequests;

public class GetApprovalRequestQueryHandlerTests
{
    private GetApprovalRequestQueryHandlerTestFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new GetApprovalRequestQueryHandlerTestFixture();
    }

    [Test]
    public async Task Handle_ThenShouldReturn_CoreValues()
    {
        var response = await _fixture.Handle();

        response.ApprovalRequests.Should().NotBeNull();
        response.ApprenticeName.Should().Be(_fixture.Apprenticeship.FirstName + " " + _fixture.Apprenticeship.LastName);
        response.ApprovalRequests.Count().Should().Be(1);

        var actual = response.ApprovalRequests.SelectMany(x => x.Items)
            .Select(x => new { x.Field, x.Old, x.New, x.Created });

        var expected = _fixture.ApprovalRequest.Items.Select(x => new { x.Field, x.Old, x.New, x.Created });
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task Handle_WithNoMatchingApprenticeshipId_ShouldReturnEmptyApprovalRequests()
    {
        var response = await _fixture.GenerateApprovalRequestsForApprenticeshipId(0).Handle();
        response.Should().BeNull();
    }

    [Test]
    public async Task Handle_WithApprenticeshipId_ShouldReturnApprovalRequests_Where_CocApprovalItemStatus_NotMatches()
    {
        var response = await _fixture.GenerateApprovalFieldRequestsForStatus(CocApprovalItemStatus.AutoRejected).Handle();
        response.ApprovalRequests.Should().BeEmpty();
    }

    public class GetApprovalRequestQueryHandlerTestFixture
    {
        public long ApprenticeshipId { get; private set; }
        public Guid ApprovalRequestId { get; private set; } = Guid.NewGuid();
        public long AccountLegalEntityId { get; private set; }
        public long AccountId { get; private set; }
        public ApprovalRequest ApprovalRequest { get; private set; }
        public List<ApprovalFieldRequest> ApprovalFieldRequests { get; private set; }
        public Apprenticeship Apprenticeship { get; private set; }
        public Cohort Cohort { get; private set; }
        public Provider Provider { get; private set; }
        public AccountLegalEntity AccountLegalEntity { get; private set; }
        public Course Course { get; private set; }

        public GetApprovalRequestQuery Request;
        public GetApprovalRequestQueryResult Result;

        private readonly GetApprovalRequestQueryHandler _handler;
        private readonly ProviderCommitmentsDbContext _db;
        private Fixture _autoFixture;

        public GetApprovalRequestQueryHandlerTestFixture()
        {
            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            SeedData();
            Request = new GetApprovalRequestQuery() { ApprenticeshipId = ApprenticeshipId, CocApprovalItemStatus = (byte)CocApprovalItemStatus.AutoApproved, AccountId = AccountId };

            _handler = new GetApprovalRequestQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
        }

        public async Task<GetApprovalRequestQueryResult> Handle()
        {
            Result = await _handler.Handle(Request, new CancellationToken());
            return Result;
        }

        private GetApprovalRequestQueryHandlerTestFixture SeedData()
        {
            _autoFixture = new Fixture();
            _autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _autoFixture.Customizations.Add(
                new TypeRelay(
                    typeof(SFA.DAS.CommitmentsV2.Models.ApprenticeshipBase),
                    typeof(Apprenticeship)));

            ApprenticeshipId = _autoFixture.Create<long>();
            AccountId = _autoFixture.Create<long>();

            Provider = new Provider
            {
                UkPrn = _autoFixture.Create<long>(),
                Name = _autoFixture.Create<string>()
            };

            var account = new Account(1, "", "", "", DateTime.UtcNow);

            AccountLegalEntity = new AccountLegalEntity(account,
                AccountLegalEntityId,
                0,
                "",
                publicHashedId: _autoFixture.Create<string>(),
                _autoFixture.Create<string>(),
                OrganisationType.PublicBodies,
                "",
                DateTime.UtcNow);

            Cohort = new Cohort
            {
                Id = _autoFixture.CreateMany<long>().Last(),
                AccountLegalEntity = AccountLegalEntity,
                EmployerAccountId = AccountId,
                ProviderId = Provider.UkPrn,
                Provider = Provider,
                ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy
            };

            var courseCode = _autoFixture.Create<string>();

            Apprenticeship = new Apprenticeship
            {
                Id = ApprenticeshipId,
                CommitmentId = Cohort.Id,
                Cohort = Cohort,
                FirstName = _autoFixture.Create<string>(),
                LastName = _autoFixture.Create<string>(),
            };

            switch (Apprenticeship.PaymentStatus)
            {
                case PaymentStatus.Withdrawn:
                    Apprenticeship.StopDate = _autoFixture.Create<DateTime>();
                    break;

                case PaymentStatus.Paused:
                    Apprenticeship.PauseDate = _autoFixture.Create<DateTime>();
                    break;

                case PaymentStatus.Completed:
                    Apprenticeship.CompletionDate = _autoFixture.Create<DateTime>();
                    break;
            }

            _db.Apprenticeships.Add(Apprenticeship);

            Course = _autoFixture.Build<Course>().With(c => c.LarsCode, courseCode).Create();
            _db.Courses.Add(Course);

            ApprovalFieldRequests = _autoFixture.Build<ApprovalFieldRequest>()
                .With(afr => afr.ApprovalRequestId, ApprovalRequestId)
                .With(afr => afr.Status, CocApprovalItemStatus.AutoApproved)
                .CreateMany(3).ToList();

            ApprovalRequest = _autoFixture.Build<ApprovalRequest>()
                .With(ar => ar.Id, ApprovalRequestId)
                .With(ar => ar.ApprenticeshipId, ApprenticeshipId)
                .With(ar => ar.Status, CocApprovalResultStatus.Pending)
                .With(ar => ar.Items, ApprovalFieldRequests)
                .Without(ar => ar.Apprenticeship)
                 .Without(x => x.EmployerAcknowledgedAt)
                .Without(x => x.EmployerAcknowledgedBy)
                .Create();
            _db.ApprovalRequests.Add(ApprovalRequest);

            _db.SaveChanges();

            return this;
        }

        public GetApprovalRequestQueryHandlerTestFixture GenerateApprovalRequestsForApprenticeshipId(long apprenticeshipId)
        {
            Request.ApprenticeshipId = apprenticeshipId;
            return this;
        }

        public GetApprovalRequestQueryHandlerTestFixture GenerateApprovalFieldRequestsForStatus(CocApprovalItemStatus status)
        {
            Request.CocApprovalItemStatus = (byte)status;
            return this;
        }
    }
}