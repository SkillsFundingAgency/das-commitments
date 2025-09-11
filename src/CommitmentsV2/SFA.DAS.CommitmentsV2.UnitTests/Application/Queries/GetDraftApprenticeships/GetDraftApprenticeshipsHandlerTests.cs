using FluentAssertions;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprenticeships;

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
    public async Task Handle_WhenCohortExists_AndRPLFeatureIsEnabledThenRecognisingPriorLearningExtendedStillNeedsToBeConsiderShouldBeFalse()
    {
        _fixture.SeedDataWithRpl2Data();
        var result = await _fixture.Handle();
        result.DraftApprenticeships.Any(x => x.RecognisingPriorLearningExtendedStillNeedsToBeConsidered).Should().BeFalse();
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
        private readonly long _cohortId;

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
                    .Build<DraftApprenticeship>()
                    .With(x => x.Id, _autoFixture.Create<long>)
                    .With(x => x.CommitmentId, _cohortId)
                    .With(x => x.StartDate, new DateTime(2022, 9, 1)) // Set start date after Aug 2022 to trigger RPL logic
                    .With(x => x.IsApproved, false)
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
            _queryResult.DraftApprenticeships.Should().HaveCount(_cohort.DraftApprenticeships.Count());

            foreach (var sourceItem in _cohort.DraftApprenticeships)
            {
                AssertEquality(sourceItem, _queryResult.DraftApprenticeships.Single(x => x.Id == sourceItem.Id));
            }
        }

        public void VerifyNoResult()
        {
            _queryResult.DraftApprenticeships.Should().BeNull();
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    private static void AssertEquality(DraftApprenticeship source, DraftApprenticeshipDto result)
    {
        result.Id.Should().Be(source.Id);
        result.FirstName.Should().Be(source.FirstName);
        result.LastName.Should().Be(source.LastName);
        result.Email.Should().Be(source.Email);
        result.DateOfBirth.Should().Be(source.DateOfBirth);
        result.Cost.Should().Be((int?)source.Cost);
        result.TrainingPrice.Should().Be(source.TrainingPrice);
        result.EndPointAssessmentPrice.Should().Be(source.EndPointAssessmentPrice);
        result.StartDate.Should().Be(source.StartDate);
        result.ActualStartDate.Should().Be(source.ActualStartDate);
        result.EndDate.Should().Be(source.EndDate);
        result.Uln.Should().Be(source.Uln);
        result.CourseCode.Should().Be(source.CourseCode);
        result.CourseName.Should().Be(source.CourseName);
        result.OriginalStartDate.Should().Be(source.OriginalStartDate);
        result.EmploymentEndDate.Should().Be(source.FlexibleEmployment.EmploymentEndDate);
        result.EmploymentPrice.Should().Be(source.FlexibleEmployment.EmploymentPrice);
        result.RecognisePriorLearning.Should().Be(source.RecognisePriorLearning);
        result.DurationReducedBy.Should().Be(source.PriorLearning.DurationReducedBy);
        result.PriceReducedBy.Should().Be(source.PriorLearning.PriceReducedBy);
        result.DurationReducedByHours.Should().Be(source.PriorLearning.DurationReducedByHours);
        result.IsOnFlexiPaymentPilot.Should().Be(source.IsOnFlexiPaymentPilot);
        result.EmployerHasEditedCost.Should().Be(source.EmployerHasEditedCost);
        result.EmailAddressConfirmed.Should().Be(source.EmailAddressConfirmed);
    }
}