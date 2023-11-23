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
using Xunit.Extensions.AssertExtensions;

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
            _fixture.SeedDataWithRpl2Data();
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        [Test]
        public async Task Handle_WhenCohortExists_AndRPLEnhancedFeatureIsEnabledThenRecognisingPriorLearningStillNeedsToBeConsiderShouldBeTrue()
        {
            _fixture.SeedDataWithRpl2Data();
            var result = await _fixture.Handle();
            result.DraftApprenticeships.Any(x=>x.RecognisingPriorLearningStillNeedsToBeConsidered).ShouldBeTrue();
        }

        [Test]
        public async Task Handle_WhenCohortExists_AndRPLFeatureIsEnabledThenRecognisingPriorLearningExtendedStillNeedsToBeConsiderShouldBeFalse()
        {
            _fixture.SeedDataWithRpl2Data();
            var result = await _fixture.Handle();
            result.DraftApprenticeships.Any(x => x.RecognisingPriorLearningExtendedStillNeedsToBeConsidered).ShouldBeFalse();
        }

        [Test]
        public async Task Handle_WhenCohortExists_AndRPLEnhancedFeatureIsNotEnabledThenRecognisingPriorLearningStillNeedsToBeConsiderShouldBeFalse()
        {
            _fixture.SeedDataWithRpl1Data();
            var result = await _fixture.Handle();
            result.DraftApprenticeships.Any(x => x.RecognisingPriorLearningStillNeedsToBeConsidered).ShouldBeFalse();
        }

        [Test]
        public async Task Handle_WhenCohortExists_AndRPLFeatureIsNotEnabledThenRecognisingPriorLearningExtendedStillNeedsToBeConsiderShouldBeTrue()
        {
            _fixture.SeedDataWithRpl1Data();
            var result = await _fixture.Handle();
            result.DraftApprenticeships.Any(x => x.RecognisingPriorLearningExtendedStillNeedsToBeConsidered).ShouldBeTrue();
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

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
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
                        .Build<DraftApprenticeship>()
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
                        .Build<DraftApprenticeship>()
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

            public void Dispose()
            {
                _db?.Dispose();
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
            Assert.AreEqual(source.TrainingPrice, result.TrainingPrice);
            Assert.AreEqual(source.EndPointAssessmentPrice, result.EndPointAssessmentPrice);
            Assert.AreEqual(source.StartDate, result.StartDate);
            Assert.AreEqual(source.ActualStartDate, result.ActualStartDate);
            Assert.AreEqual(source.EndDate, result.EndDate);
            Assert.AreEqual(source.Uln, result.Uln);
            Assert.AreEqual(source.CourseCode, result.CourseCode);
            Assert.AreEqual(source.CourseName, result.CourseName);
            Assert.AreEqual(source.OriginalStartDate, result.OriginalStartDate);
            Assert.AreEqual(source.FlexibleEmployment.EmploymentEndDate, result.EmploymentEndDate);
            Assert.AreEqual(source.FlexibleEmployment.EmploymentPrice, result.EmploymentPrice);
            Assert.AreEqual(source.RecognisePriorLearning, result.RecognisePriorLearning);
            Assert.AreEqual(source.PriorLearning.DurationReducedBy, result.DurationReducedBy);
            Assert.AreEqual(source.PriorLearning.PriceReducedBy, result.PriceReducedBy);
            Assert.AreEqual(source.PriorLearning.DurationReducedByHours, result.DurationReducedByHours);
            Assert.AreEqual(source.IsOnFlexiPaymentPilot, result.IsOnFlexiPaymentPilot);
            Assert.AreEqual(source.EmployerHasEditedCost, result.EmployerHasEditedCost);
            Assert.AreEqual(source.EmailAddressConfirmed, result.EmailAddressConfirmed);
        }
    }
}
