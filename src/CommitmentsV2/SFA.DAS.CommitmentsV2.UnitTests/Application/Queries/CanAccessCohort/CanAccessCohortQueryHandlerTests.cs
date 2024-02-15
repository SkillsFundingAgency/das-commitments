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
            using var fixtures = new CanAccessCohortQueryHandlerTestsFixture().SeedData().SetMatchingAccountQuery();

            var response = await fixtures.Handle();

            Assert.That(response, Is.True);
        }

        [Test]
        public async Task Handle_ProviderQuery_WithExistingCohort_ShouldReturnTrue()
        {
            using var fixtures = new CanAccessCohortQueryHandlerTestsFixture().SeedData().SetMatchingProviderQuery();

            var response = await fixtures.Handle();

            Assert.That(response, Is.True);
        }

        [Test]
        public async Task Handle_EmployerQuery_WithNoExistingCohort_ShouldReturnFalse()
        {
            using var fixtures = new CanAccessApprenticeshipQueryHandlerTestsFixture().SeedData().SetNonMatchingQuery();

            var response = await fixtures.Handle();

            Assert.That(response, Is.False);
        }
    }

    public class CanAccessCohortQueryHandlerTestsFixture : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public CanAccessCohortQueryHandler Handler { get; set; }
        public CanAccessCohortQuery Query { get; set; }
        private readonly long _providerId;
        private readonly long _accountId;

        private readonly Fixture _autoFixture;
        private Cohort _cohort;
        private long _cohortId;

        public CanAccessCohortQueryHandlerTestsFixture()
        {
            _autoFixture = new Fixture();
            _cohortId = _autoFixture.Create<long>();
            _providerId = _autoFixture.Create<long>();
            _accountId = _autoFixture.Create<long>();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            Handler = new CanAccessCohortQueryHandler(
                new Lazy<ProviderCommitmentsDbContext >(() => Db));
        }

        public CanAccessCohortQueryHandlerTestsFixture SetMatchingAccountQuery()
        {
            Query = new CanAccessCohortQuery { CohortId = _cohortId, Party = Party.Employer, PartyId = _accountId };
            return this;
        }

        public CanAccessCohortQueryHandlerTestsFixture SetMatchingProviderQuery()
        {
            Query = new CanAccessCohortQuery { CohortId = _cohortId, Party = Party.Provider, PartyId = _providerId };
            return this;
        }

        public CanAccessCohortQueryHandlerTestsFixture SetNonMatchingQuery()
        {
            Query = new CanAccessCohortQuery { CohortId = _cohortId + 1, Party = Party.Provider, PartyId = _accountId };
            return this;
        }

        public CanAccessCohortQueryHandlerTestsFixture SeedData()
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

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
