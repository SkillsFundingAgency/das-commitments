using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.CanAccessApprenticeship
{
    [TestFixture]
    public class CanAccessApprenticeshipQueryHandlerTests
    {
        [Test]
        public async Task Handle_EmployerQuery_WithApprovedApprenticeship_ShouldReturnTrue()
        {
            using var fixtures = new CanAccessApprenticeshipQueryHandlerTestsFixture()
                .SeedData()
                .SetMatchingAccountQuery();

            var response = await fixtures.Handle();

            Assert.That(response, Is.True);
        }

        [Test]
        public async Task Handle_ProviderQuery_WithApprovedApprenticeship_ShouldReturnTrue()
        {
            using var fixtures = new CanAccessApprenticeshipQueryHandlerTestsFixture()
                .SeedData()
                .SetMatchingProviderQuery();

            var response = await fixtures.Handle();

            Assert.That(response, Is.True);
        }

        [Test]
        public async Task Handle_EmployerQuery_WithNoApprovedApprenticeship_ShouldReturnFalse()
        {
            using var fixtures = new CanAccessApprenticeshipQueryHandlerTestsFixture()
                .SeedData()
                .SetNonMatchingQuery();

            var response = await fixtures.Handle();

            Assert.That(response, Is.False);
        }


        [Test]
        public async Task Handle_EmployerQuery_WithDraftApprenticeship_ShouldReturnFalse()
        {
            using var fixtures = new CanAccessApprenticeshipQueryHandlerTestsFixture()
                .SeedDataWithDraftApprenticeship()
                .SetMatchingAccountQuery();

            var response = await fixtures.Handle();

            Assert.That(response, Is.False);
        }
    }

    public class CanAccessApprenticeshipQueryHandlerTestsFixture : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public CanAccessApprenticeshipQueryHandler Handler { get; set; }
        public CanAccessApprenticeshipQuery Query { get; set; }
        private readonly long _providerId;
        private readonly long _accountId;
        private readonly long _apprenticeshipId;

        private readonly Fixture _autoFixture;
        private Cohort _cohort;
        private long _cohortId;

        public CanAccessApprenticeshipQueryHandlerTestsFixture()
        {
            _autoFixture = new Fixture();
            _cohortId = _autoFixture.Create<long>();
            _apprenticeshipId = _autoFixture.Create<long>();
            _providerId = _autoFixture.Create<long>();
            _accountId = _autoFixture.Create<long>();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            Handler = new CanAccessApprenticeshipQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public CanAccessApprenticeshipQueryHandlerTestsFixture SetMatchingAccountQuery()
        {
            Query = new CanAccessApprenticeshipQuery
                { ApprenticeshipId = _apprenticeshipId, Party = Party.Employer, PartyId = _accountId };
            return this;
        }

        public CanAccessApprenticeshipQueryHandlerTestsFixture SetMatchingProviderQuery()
        {
            Query = new CanAccessApprenticeshipQuery
                { ApprenticeshipId = _apprenticeshipId, Party = Party.Provider, PartyId = _providerId };
            return this;
        }

        public CanAccessApprenticeshipQueryHandlerTestsFixture SetNonMatchingQuery()
        {
            Query = new CanAccessApprenticeshipQuery
                { ApprenticeshipId = _apprenticeshipId + 1, Party = Party.Provider, PartyId = _accountId };
            return this;
        }

        public CanAccessApprenticeshipQueryHandlerTestsFixture SeedDataWithDraftApprenticeship()
        {
            return SeedData(false);
        }

        public CanAccessApprenticeshipQueryHandlerTestsFixture SeedData(bool isApproved = true)
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

            if (isApproved)
            {
                var apprenticeship = new Apprenticeship
                {
                    Id = _apprenticeshipId,
                    CommitmentId = _cohortId,
                    Cohort = _cohort
                };

                _cohort.Apprenticeships.Add(apprenticeship);

                Db.Apprenticeships.Add(apprenticeship);
            }
            else
            {
                var apprenticeship = new DraftApprenticeship()
                {
                    Id = _apprenticeshipId,
                    CommitmentId = _cohortId,
                    Cohort = _cohort
                };
                _cohort.Apprenticeships.Add(apprenticeship);
            }

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
        }
    }
}