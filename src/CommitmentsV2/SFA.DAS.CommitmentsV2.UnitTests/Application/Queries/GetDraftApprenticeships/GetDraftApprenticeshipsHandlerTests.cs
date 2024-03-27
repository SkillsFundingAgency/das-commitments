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
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_WhenCohortExists_ThenShouldReturnResult()
        {
            _fixture.SeedDataWithRpl2DraftData();
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        [Test]
        public async Task Handle_WhenCohortExists_AndRPLEnhancedFeatureIsEnabledThenRecognisingPriorLearningStillNeedsToBeConsiderShouldBeTrue()
        {
            _fixture.SeedDataWithRpl2Data();
            var result = await _fixture.Handle();
            result.DraftApprenticeships.Any(x=>x.RecognisingPriorLearningStillNeedsToBeConsidered).Should().BeTrue();
        }

        [Test]
        public async Task Handle_WhenCohortExists_AndRPLFeatureIsEnabledThenRecognisingPriorLearningExtendedStillNeedsToBeConsiderShouldBeFalse()
        {
            _fixture.SeedDataWithRpl2Data();
            var result = await _fixture.Handle();
            result.DraftApprenticeships.Any(x => x.RecognisingPriorLearningExtendedStillNeedsToBeConsidered).Should().BeFalse();
        }

        [Test]
        public async Task Handle_WhenCohortExists_AndRPLEnhancedFeatureIsNotEnabledThenRecognisingPriorLearningStillNeedsToBeConsiderShouldBeFalse()
        {
            _fixture.SeedDataWithRpl1Data();
            var result = await _fixture.Handle();
            result.DraftApprenticeships.Any(x => x.RecognisingPriorLearningStillNeedsToBeConsidered).Should().BeFalse();
        }

        [Test]
        public async Task Handle_WhenCohortExists_AndRPLFeatureIsNotEnabledThenRecognisingPriorLearningExtendedStillNeedsToBeConsiderShouldBeTrue()
        {
            _fixture.SeedDataWithRpl1Data();
            var result = await _fixture.Handle();
            result.DraftApprenticeships.Any(x => x.RecognisingPriorLearningExtendedStillNeedsToBeConsidered).Should().BeTrue();
        }

        [Test]
        public async Task Handle_WhenCohortDoesNotExist_ThenShouldNotReturnResult()
        {
            _fixture.WithNonExistentCohort();
            await _fixture.Handle();
            _fixture.VerifyNoResult();
        }

        public class GetDraftApprenticeshipsHandlerTestsFixture : IDisposable
        {
            private readonly GetDraftApprenticeshipsQueryHandler _queryHandler;
            private readonly ProviderCommitmentsDbContext _db;
            private GetDraftApprenticeshipsQuery _query;
            private GetDraftApprenticeshipsQueryResult _queryResult;
            private readonly IFixture _autoFixture;
            private Cohort _cohort;
            private long _cohortId;

            public GetDraftApprenticeshipsHandlerTestsFixture()
            {
                _autoFixture = new Fixture().Customize(new IgnoreVirtualMembersCustomisation());

                _cohortId = _autoFixture.Create<long>();
                _query = new GetDraftApprenticeshipsQuery(_cohortId);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
                _queryHandler = new GetDraftApprenticeshipsQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public GetDraftApprenticeshipsHandlerTestsFixture WithNonExistentCohort()
            {
                _query = new GetDraftApprenticeshipsQuery(_cohortId + 1);
                return this;
            }
            
            public async Task<GetDraftApprenticeshipsQueryResult> Handle()
            {
                _queryResult = await _queryHandler.Handle(TestHelper.Clone(_query), new CancellationToken());
                return _queryResult;
            }

            public void SeedDataWithRpl2Data()
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
                    var apprenticeship = _autoFixture
                        .Build<Apprenticeship>()
                        .With(x => x.Id, _autoFixture.Create<long>)
                        .With(x => x.CommitmentId, _cohortId)
                        .With(a=>a.TrainingTotalHours, 2000)
                        .With(a=>a.PriorLearning, new ApprenticeshipPriorLearning
                        {
                            DurationReducedByHours = 1000,
                            IsDurationReducedByRpl = true,
                            DurationReducedBy = 10,
                            PriceReducedBy = 240
                        })
                        .Create();

                    _cohort.Apprenticeships.Add(apprenticeship);
                }

                _db.Cohorts.Add(_cohort);
                _db.SaveChanges();
            }

            public void SeedDataWithRpl2DraftData()
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
                    var apprenticeship = _autoFixture
                        .Build<DraftApprenticeship>()
                        .With(x => x.Id, _autoFixture.Create<long>)
                        .With(x => x.CommitmentId, _cohortId)
                        .With(x=>x.IsApproved, false)
                        .With(a => a.TrainingTotalHours, 2000)
                        .With(a => a.PriorLearning, new ApprenticeshipPriorLearning
                        {
                            DurationReducedByHours = 1000,
                            IsDurationReducedByRpl = true,
                            DurationReducedBy = 10,
                            PriceReducedBy = 240
                        })
                        .Create();

                    _cohort.Apprenticeships.Add(apprenticeship);
                }

                _db.Cohorts.Add(_cohort);
                _db.SaveChanges();
            }

            public void SeedDataWithRpl1Data()
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
                    var apprenticeship = _autoFixture
                        .Build<Apprenticeship>()
                        .With(x => x.Id, _autoFixture.Create<long>)
                        .With(x => x.CommitmentId, _cohortId)
                        .With(a => a.PriorLearning, new ApprenticeshipPriorLearning
                        {
                            DurationReducedBy = 10,
                            PriceReducedBy = 240
                        })
                        .Create();

                    _cohort.Apprenticeships.Add(apprenticeship);
                }

                _db.Cohorts.Add(_cohort);
                _db.SaveChanges();
            }


            public void VerifyResultMapping()
            {
                Assert.That(_queryResult.DraftApprenticeships, Has.Count.EqualTo(_cohort.DraftApprenticeships.Count()));

                foreach (var sourceItem in _cohort.DraftApprenticeships)
                {
                    AssertEquality(sourceItem, _queryResult.DraftApprenticeships.Single(x => x.Id == sourceItem.Id));
                }
            }

            public void VerifyNoResult()
            {
                Assert.That(_queryResult.DraftApprenticeships, Is.Null);
            }

            public void Dispose()
            {
                _db?.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private static void AssertEquality(DraftApprenticeship source, DraftApprenticeshipDto result)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(source.Id));
                Assert.That(result.FirstName, Is.EqualTo(source.FirstName));
                Assert.That(result.LastName, Is.EqualTo(source.LastName));
                Assert.That(result.Email, Is.EqualTo(source.Email));
                Assert.That(result.DateOfBirth, Is.EqualTo(source.DateOfBirth));
                Assert.That(result.Cost, Is.EqualTo(source.Cost));
                Assert.That(result.TrainingPrice, Is.EqualTo(source.TrainingPrice));
                Assert.That(result.EndPointAssessmentPrice, Is.EqualTo(source.EndPointAssessmentPrice));
                Assert.That(result.StartDate, Is.EqualTo(source.StartDate));
                Assert.That(result.ActualStartDate, Is.EqualTo(source.ActualStartDate));
                Assert.That(result.EndDate, Is.EqualTo(source.EndDate));
                Assert.That(result.Uln, Is.EqualTo(source.Uln));
                Assert.That(result.CourseCode, Is.EqualTo(source.CourseCode));
                Assert.That(result.CourseName, Is.EqualTo(source.CourseName));
                Assert.That(result.OriginalStartDate, Is.EqualTo(source.OriginalStartDate));
                Assert.That(result.EmploymentEndDate, Is.EqualTo(source.FlexibleEmployment.EmploymentEndDate));
                Assert.That(result.EmploymentPrice, Is.EqualTo(source.FlexibleEmployment.EmploymentPrice));
                Assert.That(result.RecognisePriorLearning, Is.EqualTo(source.RecognisePriorLearning));
                Assert.That(result.DurationReducedBy, Is.EqualTo(source.PriorLearning.DurationReducedBy));
                Assert.That(result.PriceReducedBy, Is.EqualTo(source.PriorLearning.PriceReducedBy));
                Assert.That(result.DurationReducedByHours, Is.EqualTo(source.PriorLearning.DurationReducedByHours));
                Assert.That(result.IsOnFlexiPaymentPilot, Is.EqualTo(source.IsOnFlexiPaymentPilot));
                Assert.That(result.EmployerHasEditedCost, Is.EqualTo(source.EmployerHasEditedCost));
                Assert.That(result.EmailAddressConfirmed, Is.EqualTo(source.EmailAddressConfirmed));
            });
        }
    }
}
