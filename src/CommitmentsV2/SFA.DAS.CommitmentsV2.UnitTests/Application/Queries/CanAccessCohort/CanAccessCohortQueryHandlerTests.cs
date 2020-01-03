using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.CanAccessApprenticeship;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.CanAccessCohort
{
    [TestFixture]
    public class CanAccessCohortQueryHandlerTests
    {
        [Test]
        public async Task Handle_EmployerQuery_WithExistingCohort_ShouldReturnTrue()
        {
            var fixtures = new CanAccessCohortHandlerTestsFixture().SeedData().SetMatchingAccountQuery();

            var response = await fixtures.Handle();

            Assert.IsTrue(response);
        }

        [Test]
        public async Task Handle_ProviderQuery_WithExistingCohort_ShouldReturnTrue()
        {
            var fixtures = new CanAccessCohortHandlerTestsFixture().SeedData().SetMatchingProviderQuery();

            var response = await fixtures.Handle();

            Assert.IsTrue(response);
        }

        [Test]
        public async Task Handle_EmployerQuery_WithNoExistingCohort_ShouldReturnFalse()
        {
            var fixtures = new CanAccessApprenticeshipQueryHandlerTestsFixture().SeedData().SetNonMatchingQuery();

            var response = await fixtures.Handle();

            Assert.IsFalse(response);
        }
    }

    public class CanAccessCohortHandlerTestsFixture
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public CanAccessCohortHandler Handler { get; set; }
        public CanAccessCohortQuery Query { get; set; }
        private readonly long _providerId;
        private readonly long _accountId;

        private readonly Fixture _autoFixture;
        private Cohort _cohort;
        private long _cohortId;

        public CanAccessCohortHandlerTestsFixture()
        {
            _autoFixture = new Fixture();
            _cohortId = _autoFixture.Create<long>();
            _providerId = _autoFixture.Create<long>();
            _accountId = _autoFixture.Create<long>();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new CanAccessCohortHandler(
                new Lazy<ProviderCommitmentsDbContext >(() => Db));
        }

        public CanAccessCohortHandlerTestsFixture SetMatchingAccountQuery()
        {
            Query = new CanAccessCohortQuery { CohortId = _cohortId, Party = Party.Employer, PartyId = _accountId };
            return this;
        }

        public CanAccessCohortHandlerTestsFixture SetMatchingProviderQuery()
        {
            Query = new CanAccessCohortQuery { CohortId = _cohortId, Party = Party.Provider, PartyId = _providerId };
            return this;
        }

        public CanAccessCohortHandlerTestsFixture SetNonMatchingQuery()
        {
            Query = new CanAccessCohortQuery { CohortId = _cohortId + 1, Party = Party.Provider, PartyId = _accountId };
            return this;
        }

        public CanAccessCohortHandlerTestsFixture SeedData()
        {
            _cohort = new Cohort
            {
                EmployerAccountId = _accountId,
                CommitmentStatus = CommitmentStatus.Active,
                EditStatus = EditStatus.Both,
                LastAction = LastAction.None,
                Originator = Originator.Unknown,
                ProviderId = _providerId,
                Id = _cohortId,
            };
            Db.Cohorts.Add(_cohort);
            Db.SaveChanges();

            return this;
        }

        public Task<bool> Handle()
        {
            return Handler.Handle(Query, CancellationToken.None);
        }
    }
}
