using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Polly;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohortPriorLearningError
{
    [TestFixture]
    [Parallelizable]
    public class GetCohortPriorLearningErrorQueryHandlerTests
    {
        [Test]
        public async Task Handle_WhenNoApprenticeshipFound_ThenShouldNull()
        {
            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures();

            var result = await fixture.Handle();

            Assert.IsNull(result);
        }

        [Test]
        public async Task Handle_WhenApprenticeshipFoundButRPLSetToFalse_ThenShouldNull()
        {
            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
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
            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
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
                IsDurationReducedByRpl = true,
                DurationReducedBy = null
            };

            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
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

            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
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
                IsDurationReducedByRpl = true,
                DurationReducedBy = null
            };

            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
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

    public class GetCohortPriorLearningErrorQueryHandlerTestsFixtures
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public Mock<IFeatureTogglesService<FeatureToggle>> FeatureToggleServiceMock { get; set; }
        public GetDraftApprenticeshipPriorLearningSummaryQueryHandler Handler { get; set; }
        public ApprenticeshipPriorLearning PriorLearning { get; set; }
        public FlexibleEmployment FlexibleEmployment { get; set; }
        public Mock<IRplFundingCalulationService> RplFundingCalulationServiceMock { get; set; }
        public RplFundingCalulation RplFundingCalulation { get; set; }

        private long CohortId = 1;
        private long ApprenticeshipId = 1;

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
        {
            var autoFixture = new Fixture();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            var standardFundingPeriod = Db.Set<StandardFundingPeriod>();
            var frameworkFundingPeriods = Db.Set<FrameworkFundingPeriod>();

            RplFundingCalulation = autoFixture.Create<RplFundingCalulation>();

            RplFundingCalulationServiceMock.Setup(x => x.GetRplFundingCalulations
                                                    (It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<object>(), It.IsAny<object>())
                                                    .ReturnsAsync(RplFundingCalulation);

            Handler = new GetDraftApprenticeshipPriorLearningSummaryQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), RplFundingCalulationServiceMock.Object);

            PriorLearning = new ApprenticeshipPriorLearning { DurationReducedBy = 10, PriceReducedBy = 999, DurationReducedByHours = 9, QualificationsForRplReduction = "qualification", ReasonForRplReduction = "reason", WeightageReducedBy = 9 };
            FlexibleEmployment = new FlexibleEmployment { EmploymentEndDate = DateTime.Today, EmploymentPrice = 987 };
        }

        public Task<GetDraftApprenticeshipPriorLearningSummaryQueryResult> Handle()
        {
            var query = new GetDraftApprenticeshipPriorLearningSummaryQuery(CohortId, ApprenticeshipId);
            return Handler.Handle(query, CancellationToken.None);
        }

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures SetApprentice(ProgrammeType? programmeType, string courseCode,  DateTime? startDate)
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
                IsOnFlexiPaymentPilot = false,
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

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures SetApprenticeshipPriorLearningData(int? trainingTotalHours, ApprenticeshipPriorLearning priorLearning = null)
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

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures SetMaxFundingBandForStandard(int courseCode, int max)
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

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures SetMaxFundingBandForFramework(string courseCode, int max)
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

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures SetApprenticeshipPriorLearningToFalse()
        {
            var apprenticeship = Db.DraftApprenticeships.First();
            apprenticeship.RecognisePriorLearning = false;

            Db.SaveChanges();

            return this;
        }

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures SetApprenticeshipFlexiJob()
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

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures SetFeatureToggle(string toggleName, bool toggle)
        {
            FeatureToggleServiceMock.Setup(x => x.GetFeatureToggle(toggleName))
                .Returns(new FeatureToggle { Feature = toggleName, IsEnabled = toggle });

            return this;
        }
    }
}