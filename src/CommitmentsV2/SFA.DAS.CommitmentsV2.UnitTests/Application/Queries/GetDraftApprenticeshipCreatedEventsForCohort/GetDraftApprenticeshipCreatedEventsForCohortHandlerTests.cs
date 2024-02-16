using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort
{
    [TestFixture]
    public class GetDraftApprenticeshipCreatedEventsForCohortHandlerTests
    {
        const long CohortId = 456;
        const long ProviderId = 156;
        private readonly DateTime _loadedOn = DateTime.Now;

        private GetDraftApprenticeshipCreatedEventsForCohortHandlerTestsFixtures _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetDraftApprenticeshipCreatedEventsForCohortHandlerTestsFixtures();
        }

        [Test]
        public async Task Handle_WithValidQuery_ShouldReturnResponseWithNoCreateMessages()
        {
            _fixture.AddCohort(CohortId, ProviderId, new List<DraftApprenticeship>());

            var result = await _fixture.GetResult(new GetDraftApprenticeshipCreatedEventsForCohortQuery(ProviderId, CohortId, 0, _loadedOn));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.DraftApprenticeshipCreatedEvents.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WithValidQuery_ShouldReturnResponseWithCreateMessageForEachApprenticeAttachedToCohort()
        {
            _fixture.SetupASingleDraftApprenticeship(123).AddCohort(CohortId, ProviderId, _fixture.DraftApprentices);

            var result = await _fixture.GetResult(new GetDraftApprenticeshipCreatedEventsForCohortQuery(ProviderId, CohortId, 1, _loadedOn));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(_fixture.DraftApprentices, Has.Count.EqualTo(result.DraftApprenticeshipCreatedEvents.Length));
                Assert.That(result.DraftApprenticeshipCreatedEvents.First().CohortId, Is.EqualTo(CohortId));
            });
            Assert.That(_fixture.DraftApprentices[0].Id, Is.EqualTo(result.DraftApprenticeshipCreatedEvents.First().DraftApprenticeshipId));
            Assert.That(_fixture.DraftApprentices[0].ReservationId, Is.EqualTo(result.DraftApprenticeshipCreatedEvents.First().ReservationId));
            Assert.That(_fixture.DraftApprentices[0].Uln, Is.EqualTo(result.DraftApprenticeshipCreatedEvents.First().Uln));
            Assert.That(result.DraftApprenticeshipCreatedEvents.First().CreatedOn, Is.EqualTo(_loadedOn));
        }

        [Test]
        public void Handle_WithInvalidProviderId_ShouldThrowInvalidOperationException()
        {
            _fixture.AddCohort(CohortId, ProviderId, new List<DraftApprenticeship>());

            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.GetResult(new GetDraftApprenticeshipCreatedEventsForCohortQuery(98798, CohortId, 0, _loadedOn)));
        }

        [Test]
        public void Handle_WithWrongNumberOfApprentices_ShouldThrowInvalidOperationException()
        {
            _fixture.AddCohort(CohortId, ProviderId, new List<DraftApprenticeship>());

            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.GetResult(new GetDraftApprenticeshipCreatedEventsForCohortQuery(ProviderId, CohortId, 100, _loadedOn)));
        }
    }

    public class GetDraftApprenticeshipCreatedEventsForCohortHandlerTestsFixtures
    {
        public GetDraftApprenticeshipCreatedEventsForCohortHandlerTestsFixtures()
        {
            SeedCohorts = new List<Cohort>();
            DraftApprentices = new List<DraftApprenticeship>();
        }

        public List<DraftApprenticeship> DraftApprentices;

        public List<Cohort> SeedCohorts { get; }
        public GetDraftApprenticeshipCreatedEventsForCohortHandlerTestsFixtures AddCohort(long cohortId, long providerId, IEnumerable<DraftApprenticeship> apprentices)
        {
            var cohort = new Cohort
            {
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = EditStatus.ProviderOnly,
                LastAction = LastAction.None,
                Originator = Originator.Unknown,
                Id = cohortId,
                ProviderId = providerId,
                Reference = string.Empty
            };

            foreach (var apprentice in apprentices)
            {
                cohort.Apprenticeships.Add(apprentice);
            }

            SeedCohorts.Add(cohort);
            
            return this;
        }

        public GetDraftApprenticeshipCreatedEventsForCohortHandlerTestsFixtures SetupASingleDraftApprenticeship(long apprenticeId)
        {
            var autoFixture = new Fixture();
            var apprentice1 = autoFixture.Build<DraftApprenticeshipDetails>().Without(x => x.StartDate)
                .With(x => x.EndDate, DateTime.Today.AddYears(1)).With(x => x.DateOfBirth, DateTime.Today.AddYears(-20))
                .With(x => x.Uln, "012345678").Create();

            var draftApprenticeship = new DraftApprenticeship(apprentice1, Party.Provider);
            draftApprenticeship.Id = apprenticeId;

            DraftApprentices = new List<DraftApprenticeship> { draftApprenticeship };

            return this;
        }

        public Task<GetDraftApprenticeshipCreatedEventsForCohortQueryResult> GetResult(GetDraftApprenticeshipCreatedEventsForCohortQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetDraftApprenticeshipCreatedEventsForCohortQueryHandler(lazy);

                return handler.Handle(query, CancellationToken.None);
            });
        }

        public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(databaseName: "SFA.DAS.Commitments.Database", b => b.EnableNullChecks(false))
                .UseLoggerFactory(MyLoggerFactory)
                .Options;

            using (var dbContext = new ProviderCommitmentsDbContext(options))
            {
                dbContext.Database.EnsureCreated();
                SeedData(dbContext);
                return action(dbContext);
            }
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Cohorts.AddRange(SeedCohorts);
            dbContext.SaveChanges(true);
        }

        private static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }
}