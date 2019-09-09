using System;
using System.Collections.Generic;
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
        public async Task Handle_WhenCohortDoesNotExist_ThenShouldReturnResult()
        {
            _fixture.WithNonExistentCohort();
            await _fixture.Handle();
            _fixture.VerifyNoResult();
        }

        public class GetDraftApprenticeshipsHandlerTestsFixture
        {
            private readonly GetDraftApprenticeshipsHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private GetDraftApprenticeshipsRequest _request;
            private GetDraftApprenticeshipsResult _result;
            private readonly Fixture _autoFixture;
            private Cohort _cohort;
            private long _cohortId;

            public GetDraftApprenticeshipsHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                _cohortId = _autoFixture.Create<long>();
                _request = new GetDraftApprenticeshipsRequest(_cohortId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                SeedData();
                _handler = new GetDraftApprenticeshipsHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public GetDraftApprenticeshipsHandlerTestsFixture WithNonExistentCohort()
            {
                _request = new GetDraftApprenticeshipsRequest(_cohortId+1);
                return this;
            }

            public async Task<GetDraftApprenticeshipsResult> Handle()
            {
                _result = await _handler.Handle(TestHelper.Clone(_request), new CancellationToken());
                return _result;
            }

            private void SeedData()
            {
                _cohort = new Cohort
                {
                    LegalEntityId = _autoFixture.Create<string>(),
                    LegalEntityName = _autoFixture.Create<string>(),
                    LegalEntityAddress = _autoFixture.Create<string>(),
                    LegalEntityOrganisationType = _autoFixture.Create<OrganisationType>(),
                    CommitmentStatus = CommitmentStatus.New,
                    EditStatus = EditStatus.EmployerOnly,
                    LastAction = LastAction.None,
                    Originator = Originator.Unknown,
                    ProviderName = _autoFixture.Create<string>(),
                    Id = _cohortId,
                    Reference = string.Empty,
                    AccountLegalEntityPublicHashedId = _autoFixture.Create<string>()
                };

                var apprenticeships = new List<DraftApprenticeship>();

                for (var i = 0; i < 10; i++)
                {
                    var apprenticeship = new DraftApprenticeship
                    {
                        CommitmentId = _cohortId,
                        Cohort = _cohort,
                        AgreementStatus = AgreementStatus.NotAgreed,
                        FirstName = _autoFixture.Create<string>(),
                        LastName = _autoFixture.Create<string>(),
                        Cost = _autoFixture.Create<int?>(),
                        DateOfBirth = _autoFixture.Create<DateTime?>(),
                        StartDate = _autoFixture.Create<DateTime?>(),
                        EndDate = _autoFixture.Create<DateTime?>(),
                        CourseCode = _autoFixture.Create<string>(),
                        CourseName = _autoFixture.Create<string>(),
                        Uln = _autoFixture.Create<string>()
                    };
                    apprenticeships.Add(apprenticeship);
                }

                _db.Cohorts.Add(_cohort);
                _db.SaveChanges();
            }

            public void VerifyResultMapping()
            {
                Assert.AreEqual(_cohort.DraftApprenticeships.Count(), _result.DraftApprenticeships.Count);

                foreach (var sourceItem in _cohort.DraftApprenticeships)
                {
                    AssertEquality(sourceItem, _result.DraftApprenticeships.Single(x => x.Id == sourceItem.Id));
                }
            }

            public void VerifyNoResult()
            {
                Assert.IsNull(_result.DraftApprenticeships);
            }
        }

        private static void AssertEquality(DraftApprenticeship source, DraftApprenticeshipDto result)
        {
            Assert.AreEqual(source.Id, result.Id);
            Assert.AreEqual(source.FirstName, result.FirstName);
            Assert.AreEqual(source.LastName, result.LastName);
            Assert.AreEqual(source.DateOfBirth, result.DateOfBirth);
            Assert.AreEqual(source.Cost, result.Cost);
            Assert.AreEqual(source.StartDate, result.StartDate);
            Assert.AreEqual(source.EndDate, result.EndDate);
            Assert.AreEqual(source.Uln, result.Uln);
            Assert.AreEqual(source.CourseCode, result.CourseCode);
            Assert.AreEqual(source.CourseName, result.CourseName);
        }
    }
}
