using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingLearnerChangeCount;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetPendingLearnerChangeCount
{
    [TestFixture]
    public class GetPendingLearnerChangeCountsForEmployerQueryHandlerTests
    {
        private GetPendingLearnerChangeCountsForEmployerQueryHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetPendingLearnerChangeCountsForEmployerQueryHandlerTestsFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_ThenShouldFindNoPendingRequests()
        {
            var result = await _fixture.Handle(222);

            result.ManualPendingChangeCount.Should().Be(0);
            result.IlrPendingChangeCount.Should().Be(0);
        }

        [Test]
        public async Task Handle_ThenShouldFindPendingManualRequests()
        {
            var result = await _fixture.AddApprenticeshipUpdates().Handle(222);

            result.ManualPendingChangeCount.Should().Be(2);
            result.IlrPendingChangeCount.Should().Be(0);
        }

        [Test]
        public async Task Handle_ThenShouldFindPendingIlrRequests()
        {
            var result = await _fixture.AddIlrUpdates().Handle(222);

            result.ManualPendingChangeCount.Should().Be(0);
            result.IlrPendingChangeCount.Should().Be(2);
        }

        [Test]
        public async Task Handle_ThenShouldFindPendingIlrAndManualRequests()
        {
            var result = await _fixture.AddApprenticeshipUpdates().AddIlrUpdates().Handle(222);

            result.ManualPendingChangeCount.Should().Be(2);
            result.IlrPendingChangeCount.Should().Be(2);
        }

        [Test]
        public async Task Handle_ThenShouldNotFindPendingIlrAndManualRequestsWhenLookingForAnotherEmployer()
        {
            var result = await _fixture.AddApprenticeshipUpdates().AddIlrUpdates().Handle(123);

            result.ManualPendingChangeCount.Should().Be(0);
            result.IlrPendingChangeCount.Should().Be(0);
        }

        [Test]
        public async Task Handle_ThenCountsOneApprenticeshipWhenItHasTwoPendingFields()
        {
            var result = await _fixture.AddTwoPendingFieldsOnOneApprenticeship().Handle(222);

            result.IlrPendingChangeCount.Should().Be(1);
        }

        [TestCase(CocApprovalItemStatus.AutoApproved)]
        [TestCase(CocApprovalItemStatus.Superseded)]
        [TestCase(CocApprovalItemStatus.Reversal)]
        public async Task Handle_ThenShouldNotFindPendingIlrRequestsWhenStatusIs(CocApprovalItemStatus status)
        {
            var result = await _fixture.AddIlrUpdates(status).Handle(222);

            result.IlrPendingChangeCount.Should().Be(0);
        }

        [TestCase(ApprenticeshipUpdateStatus.Approved)]
        [TestCase(ApprenticeshipUpdateStatus.Rejected)]
        [TestCase(ApprenticeshipUpdateStatus.Deleted)]
        [TestCase(ApprenticeshipUpdateStatus.Superceded)]
        [TestCase(ApprenticeshipUpdateStatus.Expired)]
        public async Task Handle_ThenShouldNotFindPendingManualRequestsWhenStatusIs(ApprenticeshipUpdateStatus status)
        {
            var result = await _fixture.AddApprenticeshipUpdates(status).Handle(222);

            result.ManualPendingChangeCount.Should().Be(0);
        }

        public class GetPendingLearnerChangeCountsForEmployerQueryHandlerTestsFixture : IDisposable
        {
            private readonly GetPendingLearnerChangeCountsForEmployerQueryHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private GetPendingLearnerChangeCountsForEmployerQueryResult _result;
            private readonly Fixture _autoFixture;

            public GetPendingLearnerChangeCountsForEmployerQueryHandlerTestsFixture()
            {
                _autoFixture = new Fixture();
                _autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);

                SeedData();

                _handler = new GetPendingLearnerChangeCountsForEmployerQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public async Task<GetPendingLearnerChangeCountsForEmployerQueryResult> Handle(long employerAccountId)
            {
                _result = await _handler.Handle(new GetPendingLearnerChangeCountsForEmployerQuery(employerAccountId), new CancellationToken());
                return _result;
            }


            public void SeedData()
            {
                var cohort = new Cohort()
                   .Set(c => c.Id, 111)
                   .Set(c => c.EmployerAccountId, 222)
                   .Set(c => c.ProviderId, 333)
                   .Set(c => c.AccountLegalEntity, new AccountLegalEntity());

                for (var i = 1001; i < 1005; i++)
                {
                    var apprenticeship = _autoFixture.Build<Apprenticeship>()
                     .With(s => s.Cohort, cohort)
                     .With(s => s.PaymentStatus, PaymentStatus.Active)
                     .With(s => s.EndDate, DateTime.UtcNow.AddYears(1))
                     .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
                     .Without(s => s.DataLockStatus)
                     .Without(s => s.EpaOrg)
                     .Without(s => s.ApprenticeshipUpdate)
                     .Without(s => s.ApprovalRequests)
                     .Without(s => s.CocApprovalItems)
                     .Without(s => s.Continuation)
                     .Without(s => s.PreviousApprenticeship)
                     .Without(s => s.CompletionDate)
                     .Without(s => s.EmailAddressConfirmed)
                     .Without(s => s.ApprenticeshipConfirmationStatus)
                     .Create();
                    cohort.Apprenticeships.Add(apprenticeship);
                    _db.Apprenticeships.Add(apprenticeship);
                }

                _db.Cohorts.Add(cohort);
                _db.SaveChanges();
            }

            public GetPendingLearnerChangeCountsForEmployerQueryHandlerTestsFixture AddApprenticeshipUpdates(
                ApprenticeshipUpdateStatus withStatus = ApprenticeshipUpdateStatus.Pending)
            {

                var apprenticeship1 = _db.Apprenticeships.First();
                _db.ApprenticeshipUpdates.Add(CreateManualUpdate(apprenticeship1));
                var apprenticeship2 = _db.Apprenticeships.Last();
                _db.ApprenticeshipUpdates.Add(CreateManualUpdate(apprenticeship2));

                _db.SaveChanges();
                return this;

                ApprenticeshipUpdate CreateManualUpdate(Apprenticeship apprenticeship) => _autoFixture.Build<ApprenticeshipUpdate>()
                     .With(s => s.ApprenticeshipId, apprenticeship.Id)
                     .With(s => s.Originator, Originator.Provider)
                     .With(s => s.Status, withStatus)
                     .With(s => s.Apprenticeship, apprenticeship)
                     .Without(s => s.DataLockStatus)
                     .Create();
            }

            public GetPendingLearnerChangeCountsForEmployerQueryHandlerTestsFixture AddIlrUpdates(
                CocApprovalItemStatus withStatus = CocApprovalItemStatus.Pending)
            {
                var apprenticeship1 = _db.Apprenticeships.First();
                _db.CocApprovalItems.Add(CreateFieldRow(apprenticeship1, withStatus, "PlannedEndDate"));
                var apprenticeship2 = _db.Apprenticeships.Last();
                _db.CocApprovalItems.Add(CreateFieldRow(apprenticeship2, withStatus, "PlannedEndDate"));

                _db.SaveChanges();
                return this;
            }

            public GetPendingLearnerChangeCountsForEmployerQueryHandlerTestsFixture AddTwoPendingFieldsOnOneApprenticeship()
            {
                var apprenticeship = _db.Apprenticeships.First();
                _db.CocApprovalItems.Add(CreateFieldRow(apprenticeship, CocApprovalItemStatus.Pending, "PlannedEndDate"));
                _db.CocApprovalItems.Add(CreateFieldRow(apprenticeship, CocApprovalItemStatus.Pending, "Prices"));
                _db.SaveChanges();
                return this;
            }

            private CocApprovalItem CreateFieldRow(Apprenticeship apprenticeship, CocApprovalItemStatus status, string field)
            {
                return new CocApprovalItem
                {
                    Id = Guid.NewGuid(),
                    ApprenticeshipId = apprenticeship.Id,
                    Status = status,
                    Field = field,
                    Old = "old",
                    New = "new",
                    LearningKey = Guid.NewGuid(),
                    LearningType = CocLearningType.Apprenticeship,
                    UKPRN = "10000000",
                    ULN = "1234567890"
                };
            }

            public void Dispose()
            {
                _db?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}