using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprenticeships
{
    [TestFixture]
    public class GetDraftApprenticeshipsHandlerTests
    {
        private GetDraftApprenticeshipsHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetDraftApprenticeshipsHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenCohortExists_ThenShouldReturnResult()
        {
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        [Test]
        public async Task Handle_WhenCohortDoesNotExist_ThenShouldNotReturnResult()
        {
            _fixture.WithNonExistentCohort();
            await _fixture.Handle();
            _fixture.VerifyNoResult();
        }

        public class GetDraftApprenticeshipsHandlerTestsFixture
        {
            private readonly GetDraftApprenticeshipsQueryHandler _queryHandler;
            private readonly ProviderCommitmentsDbContext _db;
            private GetDraftApprenticeshipsQuery _query;
            private GetDraftApprenticeshipsQueryResult _queryResult;
            private readonly Fixture _autoFixture;
            private Cohort _cohort;
            private long _cohortId;

            public GetDraftApprenticeshipsHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                _cohortId = _autoFixture.Create<long>();
                _query = new GetDraftApprenticeshipsQuery(_cohortId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                SeedData();
                _queryHandler = new GetDraftApprenticeshipsQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public GetDraftApprenticeshipsHandlerTestsFixture WithNonExistentCohort()
            {
                _query = new GetDraftApprenticeshipsQuery(_cohortId+1);
                return this;
            }

            public GetDraftApprenticeshipsHandlerTestsFixture WithDeletedCohort()
            {
                _cohort.IsDeleted = true;
                return this;
            }

            public async Task<GetDraftApprenticeshipsQueryResult> Handle()
            {
                _queryResult = await _queryHandler.Handle(TestHelper.Clone(_query), new CancellationToken());
                return _queryResult;
            }

            private void SeedData()
            {
                _cohort = new Cohort
                {
                    CommitmentStatus = CommitmentStatus.New,
                    EditStatus = EditStatus.EmployerOnly,
                    LastAction = LastAction.None,
                    Originator = Originator.Unknown,
                    Id = _cohortId,
                    Reference = string.Empty
                };

                for (var i = 0; i < 10; i++)
                {
                    var apprenticeship = new DraftApprenticeship
                    {
                        CommitmentId = _cohortId,
                        Cohort = _cohort,
                        FirstName = _autoFixture.Create<string>(),
                        LastName = _autoFixture.Create<string>(),
                        Email = _autoFixture.Create<string>(),
                        Cost = _autoFixture.Create<int?>(),
                        DateOfBirth = _autoFixture.Create<DateTime?>(),
                        StartDate = _autoFixture.Create<DateTime?>(),
                        EndDate = _autoFixture.Create<DateTime?>(),
                        CourseCode = _autoFixture.Create<string>(),
                        CourseName = _autoFixture.Create<string>(),
                        Uln = _autoFixture.Create<string>(),
                        OriginalStartDate = _autoFixture.Create<DateTime?>()
                    };
                    _cohort.Apprenticeships.Add(apprenticeship);
                }

                _db.Cohorts.Add(_cohort);
                _db.SaveChanges();
            }

            public void VerifyResultMapping()
            {
                Assert.AreEqual(_cohort.DraftApprenticeships.Count(), _queryResult.DraftApprenticeships.Count);

                foreach (var sourceItem in _cohort.DraftApprenticeships)
                {
                    AssertEquality(sourceItem, _queryResult.DraftApprenticeships.Single(x => x.Id == sourceItem.Id));
                }
            }

            public void VerifyNoResult()
            {
                Assert.IsNull(_queryResult.DraftApprenticeships);
            }
        }

        private static void AssertEquality(DraftApprenticeship source, DraftApprenticeshipDto result)
        {
            Assert.AreEqual(source.Id, result.Id);
            Assert.AreEqual(source.FirstName, result.FirstName);
            Assert.AreEqual(source.LastName, result.LastName);
            Assert.AreEqual(source.Email, result.Email);
            Assert.AreEqual(source.DateOfBirth, result.DateOfBirth);
            Assert.AreEqual(source.Cost, result.Cost);
            Assert.AreEqual(source.StartDate, result.StartDate);
            Assert.AreEqual(source.EndDate, result.EndDate);
            Assert.AreEqual(source.Uln, result.Uln);
            Assert.AreEqual(source.CourseCode, result.CourseCode);
            Assert.AreEqual(source.CourseName, result.CourseName);
            Assert.AreEqual(source.OriginalStartDate, result.OriginalStartDate);
        }
    }
}
