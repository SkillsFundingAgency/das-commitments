using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortPriorLearningError;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
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
        public async Task Handle_WhenApprenticeshipFoundAndRPLSetToTrueAndInCohort_ThenResultContainsApprenticeship()
        {
            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
                .SetApprentice(ProgrammeType.Standard, "123", DateTime.Today)
                .SetApprenticeshipPriorLearningData(10, new ApprenticeshipPriorLearning() { DurationReducedBy = 10, DurationReducedByHours = 1, PriceReducedBy = 10, WeightageReducedBy = 10, QualificationsForRplReduction = "quals", ReasonForRplReduction = "reason", IsDurationReducedByRpl = true });

            var result = await fixture.Handle();

            Assert.That(result.DraftApprenticeshipIds, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task Handle_WhenApprenticeshipFoundAndRPLSetToFalseAndInCohort_ThenResultDoesntContainsApprenticeship()
        {
            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
                .SetApprentice(ProgrammeType.Standard, "123", DateTime.Today);

            var result = await fixture.Handle();

            Assert.That(result.DraftApprenticeshipIds, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task Handle_HandleReturnedTypeIsCorrect()
        {
            var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures();

            var result = await fixture.Handle();

            Assert.IsInstanceOf<GetCohortPriorLearningErrorQueryResult>(result);
        }

    }

    public class GetCohortPriorLearningErrorQueryHandlerTestsFixtures
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public Mock<IFeatureTogglesService<FeatureToggle>> FeatureToggleServiceMock { get; set; }
        public GetCohortPriorLearningErrorQueryHandler Handler { get; set; }
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

            RplFundingCalulation = autoFixture.Create<RplFundingCalulation>();

            RplFundingCalulationServiceMock = new Mock<IRplFundingCalulationService>();
            RplFundingCalulationServiceMock.Setup(x => x.GetRplFundingCalulations
                                                    (It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DbSet<StandardFundingPeriod>>(), 
                                                        It.IsAny<DbSet<FrameworkFundingPeriod>>())).ReturnsAsync(RplFundingCalulation);

            Handler = new GetCohortPriorLearningErrorQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), RplFundingCalulationServiceMock.Object);

            PriorLearning = new ApprenticeshipPriorLearning { DurationReducedBy = 10, PriceReducedBy = 999, DurationReducedByHours = 9, QualificationsForRplReduction = "qualification", ReasonForRplReduction = "reason", WeightageReducedBy = 9 };
            FlexibleEmployment = new FlexibleEmployment { EmploymentEndDate = DateTime.Today, EmploymentPrice = 987 };
        }

        public Task<GetCohortPriorLearningErrorQueryResult> Handle()
        {
            var query = new GetCohortPriorLearningErrorQuery(CohortId);
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