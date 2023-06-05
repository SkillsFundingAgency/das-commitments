using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprenticePriorLearningSummary
{
    [TestFixture]
    [Parallelizable]
    public class GetDraftApprenticePriorLearningSummaryQueryHandlerTests
    {
        [Test]
        public async Task Handle_WhenNoApprenticeshipFound_ThenShouldNull()
        {
            var fixture = new GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures();

            var result = await fixture.Handle();

            Assert.IsNull(result);
        }

        [Test]
        public async Task Handle_WhenApprenticeshipFoundButRPLSetToFalse_ThenShouldNull()
        {
            var fixture = new GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures()
                .SetApprentice(ProgrammeType.Standard, "123", DateTime.Today)
                .SetApprenticeshipPriorLearningToFalse();

            var result = await fixture.Handle();

            Assert.IsNull(result);
        }

        [TestCase(null, null)]
        [TestCase("123", ProgrammeType.Framework)]
        [TestCase("123-123", ProgrammeType.Framework)]
        public async Task Handle_WhenApprenticeshipFoundAndNoMaxFundingFoundFundingForCourseCode_ThenMaxFundingShouldBeNullAndNoErrorDisplayed(string courseCode, ProgrammeType? programmeType)
        {
            var fixture = new GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures()
                .SetApprentice(programmeType, courseCode, DateTime.Today)
                .SetApprenticeshipPriorLearningData(1000);

            var result = await fixture.Handle();

            Assert.IsNull(result.FundingBandMaximum);
        }

        [TestCase(3200, 2000,  false)]
        [TestCase(8200, 800, true)]
        public async Task Handle_WhenApprenticeshipFoundAndMaxFundingFoundForStandard_ThenRPLSummaryDataShouldBeReturnedWithCorrectValues(
            int maxFundingBand, int trainingTotalHours, bool reductionIsInError)
        {
            var priorLearning = new ApprenticeshipPriorLearning
            {
                DurationReducedByHours = 200,
                PriceReducedBy = 1000,
                IsDurationReducedByRpl = false,
                DurationReducedBy = null
            };

            var fixture = new GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures()
                .SetApprentice(ProgrammeType.Standard, "1", DateTime.Today)
                .SetMaxFundingBandForStandard(1, maxFundingBand)
                .SetApprenticeshipPriorLearningData(trainingTotalHours, priorLearning);

            var result = await fixture.Handle();

            Assert.AreEqual(maxFundingBand, result.FundingBandMaximum);
            Assert.AreEqual((decimal)priorLearning.DurationReducedByHours / trainingTotalHours * 100, result.PercentageOfPriorLearning);
            Assert.AreEqual((decimal)priorLearning.DurationReducedByHours / trainingTotalHours * 100 / 2, result.MinimumPercentageReduction);
            Assert.AreEqual(reductionIsInError, result.RplPriceReductionError);
        }

        [TestCase(null,  false)]
        [TestCase(0, false)]
        [TestCase(1000, true)]
        [TestCase(null, false)]
        [TestCase(10000, true)]
        public async Task Handle_WhenApprenticeshipFoundAndHasoldRPLValues_ThenRPLSummaryDataShouldBeReturnedWithNullValuesAnd(
            int? trainingTotalHours, bool expectedToHaveAValue)
        {
            var priorLearning = new ApprenticeshipPriorLearning
            {
                DurationReducedByHours = 200,
                PriceReducedBy = 1000
            };

            var fixture = new GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures()
                .SetApprentice(ProgrammeType.Standard, "1", DateTime.Today)
                .SetMaxFundingBandForStandard(1, 5000)
                .SetApprenticeshipPriorLearningData(trainingTotalHours, priorLearning);

            var result = await fixture.Handle();

            Assert.AreEqual(expectedToHaveAValue, result.PercentageOfPriorLearning.HasValue);
            Assert.AreEqual(expectedToHaveAValue, result.MinimumPercentageReduction.HasValue);
            Assert.AreEqual(expectedToHaveAValue, result.MinimumPriceReduction.HasValue);
            Assert.IsFalse(result.RplPriceReductionError);
        }

        [TestCase(3200, 2000, false)]
        [TestCase(8200, 800, true)]
        public async Task Handle_WhenApprenticeshipFounaxFundingFoundForFramework_ThenRPLSummaryDataShouldBeReturnedWithCorrectValues(
            int maxFundingBand, int trainingTotalHours, bool reductionIsInError)
        {
            var priorLearning = new ApprenticeshipPriorLearning
            {
                DurationReducedByHours = 200,
                PriceReducedBy = 1000,
                IsDurationReducedByRpl = false,
                DurationReducedBy = null
            };

            var fixture = new GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures()
                .SetApprentice(ProgrammeType.Framework, "123-123", DateTime.Today)
                .SetMaxFundingBandForFramework("123-123", maxFundingBand)
                .SetApprenticeshipPriorLearningData(trainingTotalHours, priorLearning);

            var result = await fixture.Handle();

            Assert.AreEqual(maxFundingBand, result.FundingBandMaximum);
            Assert.AreEqual((decimal)priorLearning.DurationReducedByHours / trainingTotalHours * 100, result.PercentageOfPriorLearning);
            Assert.AreEqual((decimal)priorLearning.DurationReducedByHours / trainingTotalHours * 100 / 2, result.MinimumPercentageReduction);
            Assert.AreEqual(reductionIsInError, result.RplPriceReductionError);
        }
    }

    public class GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public Mock<IFeatureTogglesService<FeatureToggle>> FeatureToggleServiceMock { get; set; }
        public GetDraftApprenticeshipPriorLearningSummaryQueryHandler Handler { get; set; }
        public ApprenticeshipPriorLearning PriorLearning { get; set; }
        public FlexibleEmployment FlexibleEmployment { get; set; }

        private long CohortId = 1;
        private long ApprenticeshipId = 1;

        public GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new GetDraftApprenticeshipPriorLearningSummaryQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db));

            PriorLearning = new ApprenticeshipPriorLearning { DurationReducedBy = 10, PriceReducedBy = 999, DurationReducedByHours = 9, QualificationsForRplReduction = "qualification", ReasonForRplReduction = "reason", WeightageReducedBy = 9 };
            FlexibleEmployment = new FlexibleEmployment { EmploymentEndDate = DateTime.Today, EmploymentPrice = 987 };
        }

        public Task<GetDraftApprenticeshipPriorLearningSummaryQueryResult> Handle()
        {
            var query = new GetDraftApprenticeshipPriorLearningSummaryQuery(CohortId, ApprenticeshipId);
            return Handler.Handle(query, CancellationToken.None);
        }

        public GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures SetApprentice(ProgrammeType? programmeType, string courseCode,  DateTime? startDate)
        {
            // This line is required.
            // ReSharper disable once ObjectCreationAsStatement
            new UnitOfWorkContext();

            var autoFixture = new Fixture();

            CommitmentsV2.Domain.Entities.TrainingProgramme trainingProgramme = null;
            if (programmeType.HasValue)
            {
                trainingProgramme = new TrainingProgramme(courseCode, "SomeName", programmeType.Value, startDate, null);
            }

            var draftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                Reference = "reference",
                Id = ApprenticeshipId,
                FirstName = "AFirstName",
                LastName = "ALastName",
                DeliveryModel = DeliveryModel.Regular,
                StartDate = startDate,
                TrainingProgramme = trainingProgramme,
                IsOnFlexiPaymentPilot = false
            };

            var commitment = new Cohort(
                autoFixture.Create<long>(),
                autoFixture.Create<long>(),
                autoFixture.Create<long>(),
                null,
                null,
                draftApprenticeshipDetails,
                Party.Provider,
                new UserInfo());

            Db.Cohorts.Add(commitment);

            Db.SaveChanges();

            ApprenticeshipId = commitment.Apprenticeships.First().Id;

            CohortId = commitment.Id;

            return this;
        }

        public GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures SetApprenticeshipPriorLearningData(int? trainingTotalHours, ApprenticeshipPriorLearning priorLearning = null)
        {
            if (priorLearning != null)
            {
                PriorLearning = priorLearning;
            }

            var apprenticeship = Db.DraftApprenticeships.First();
            apprenticeship.RecognisePriorLearning = true;
            apprenticeship.PriorLearning = PriorLearning;
            apprenticeship.TrainingTotalHours = trainingTotalHours;
            

            Db.SaveChanges();

            return this;
        }

        public GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures SetMaxFundingBandForStandard(int courseCode, int max)
        {
            Db.StandardFundingPeriods.Add(new StandardFundingPeriod
            {
                Id = courseCode,
                EffectiveFrom = DateTime.Today.AddDays(-1),
                FundingCap = max,
            });

            Db.SaveChanges();

            return this;
        }

        public GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures SetMaxFundingBandForFramework(string courseCode, int max)
        {
            Db.FrameworkFundingPeriods.Add(new FrameworkFundingPeriod
            {
                Id = courseCode,
                EffectiveFrom = DateTime.Today.AddDays(-1),
                FundingCap = max,
            });

            Db.SaveChanges();

            return this;
        }

        public GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures SetApprenticeshipPriorLearningToFalse()
        {
            var apprenticeship = Db.DraftApprenticeships.First();
            apprenticeship.RecognisePriorLearning = false;

            Db.SaveChanges();

            return this;
        }

        public GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures SetApprenticeshipFlexiJob()
        {
            var apprenticeship = Db.DraftApprenticeships.First();
            apprenticeship.FlexibleEmployment = FlexibleEmployment;
            apprenticeship.DeliveryModel = DeliveryModel.PortableFlexiJob;

            Db.SaveChanges();

            return this;
        }

        public DraftApprenticeship GetDraftApprenticeship()
        {
            return Db.DraftApprenticeships.First();
        }

        public GetDraftApprenticePriorLearningSummaryQueryHandlerTestsFixtures SetFeatureToggle(string toggleName, bool toggle)
        {
            FeatureToggleServiceMock.Setup(x => x.GetFeatureToggle(toggleName))
                .Returns(new FeatureToggle { Feature = toggleName, IsEnabled = toggle });

            return this;
        }
    }
}