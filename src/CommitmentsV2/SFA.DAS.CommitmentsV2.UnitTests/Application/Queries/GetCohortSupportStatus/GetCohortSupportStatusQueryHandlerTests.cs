using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSupportStatus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohortSupportStatus;

[TestFixture]
public class GetCohortSupportStatusQueryHandlerTests
{
    [Test]
    public async Task CohortSupportStatus_Is_Returned_Correctly()
    {
        using var fixture = new GetCohortSupportStatusQueryHandlerTestsFixture();
        fixture.SeedData();
        var result = await fixture.Handle();
        result.CohortId.Should().Be(fixture.Cohort.Id);
        result.NoOfApprentices.Should().Be(fixture.Cohort.Apprenticeships.Count);
        result.CohortStatus.Should().Be("Pending");
    }

    [Test]
    public async Task CohortSupportStatus_Returns_Null_When_No_Data()
    {
        using var fixture = new GetCohortSupportStatusQueryHandlerTestsFixture();
        var result = await fixture.Handle();
        result.Should().BeNull();
    }

    [Test]
    public async Task CohortSupportStatus_Calls_GetStatus_Correctly()
    {
        using var fixture = new GetCohortSupportStatusQueryHandlerTestsFixture();
        fixture.SeedData();
        var result = await fixture.Handle();
        fixture.VerifyCalculatorIsCalledWithExpectedParameters();
    }

    private class GetCohortSupportStatusQueryHandlerTestsFixture : IDisposable
    {
        private long _cohortId;
        public Cohort Cohort;
        private ProviderCommitmentsDbContext _db { get; set; }
        private Mock<ICohortSupportStatusCalculator> _calculator { get; set; }
        private GetCohortSupportStatusQueryHandler _sut { get; set; }
        private GetCohortSupportStatusQuery _query;
        private Fixture _fixture;

        public GetCohortSupportStatusQueryHandlerTestsFixture()
        {

            _fixture = new Fixture();
            _cohortId = _fixture.Create<long>();
            _query = new GetCohortSupportStatusQuery(_cohortId);

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            _calculator = new Mock<ICohortSupportStatusCalculator>();
            _calculator.Setup(x => x.GetStatus(It.IsAny<EditStatus>(), It.IsAny<bool>(), It.IsAny<LastAction>(),
                    It.IsAny<Party>(), It.IsAny<long?>(), It.IsAny<TransferApprovalStatus?>()))
                .Returns(RequestSupportStatus.WithSenderForApproval);

            _sut = new GetCohortSupportStatusQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), _calculator.Object);
 
        }

        public GetCohortSupportStatusQueryHandlerTestsFixture SeedData()
        {
            var cohort = new Cohort();
            cohort.Id = _cohortId;
            cohort.WithParty = _fixture.Create<Party>();
            cohort.EditStatus = _fixture.Create<EditStatus>();
            cohort.LastAction = _fixture.Create<LastAction>();
            cohort.TransferSenderId = _fixture.Create<long?>();
            cohort.TransferApprovalStatus = _fixture.Create<TransferApprovalStatus?>();
            cohort.Approvals = Party.None;

            _db.Cohorts.Add(cohort);
            _db.SaveChanges();

            Cohort = cohort;

            return this;
        }

        public GetCohortSupportStatusQueryHandlerTestsFixture VerifyCalculatorIsCalledWithExpectedParameters()
        {
            _calculator.Verify(x => x.GetStatus(Cohort.EditStatus, Cohort.Apprenticeships.Count > 0, Cohort.LastAction,
                Cohort.WithParty, Cohort.TransferSenderId, Cohort.TransferApprovalStatus));

            return this;
        }

        public async Task<GetCohortSupportStatusQueryResult> Handle()
        {
            return await _sut.Handle(_query, CancellationToken.None);
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}