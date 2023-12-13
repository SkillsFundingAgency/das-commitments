using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortPriorLearningError;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            using var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
                .SetApprentice(ProgrammeType.Standard, "123", DateTime.Today)
                .SetApprenticeshipPriorLearningData(10, new ApprenticeshipPriorLearning() { DurationReducedBy = 10, DurationReducedByHours = 1, PriceReducedBy = 10, IsDurationReducedByRpl = true });

            var result = await fixture.Handle();

            Assert.That(result.DraftApprenticeshipIds, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task Handle_WhenApprenticeshipFoundAndRPLSetToFalseAndInCohort_ThenResultDoesntContainsApprenticeship()
        {
            using var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
                .SetApprentice(ProgrammeType.Standard, "123", DateTime.Today);

            var result = await fixture.Handle();

            Assert.That(result.DraftApprenticeshipIds, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task Handle_HandleReturnedTypeIsCorrect()
        {
            using var fixture = new GetCohortPriorLearningErrorQueryHandlerTestsFixtures();

            var result = await fixture.Handle();

            Assert.IsInstanceOf<GetCohortPriorLearningErrorQueryResult>(result);
        }

    }

    public class GetCohortPriorLearningErrorQueryHandlerTestsFixtures : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public GetCohortPriorLearningErrorQueryHandler Handler { get; set; }
        public ApprenticeshipPriorLearning PriorLearning { get; set; }
        public FlexibleEmployment FlexibleEmployment { get; set; }
        public Mock<IRplFundingCalculationService> RplFundingCalculationServiceMock { get; set; }
        public RplFundingCalculation RplFundingCalculation { get; set; }

        private long CohortId = 1;
        private long ApprenticeshipId = 1;

        public GetCohortPriorLearningErrorQueryHandlerTestsFixtures()
        {
            var autoFixture = new Fixture();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            RplFundingCalculation = autoFixture.Create<RplFundingCalculation>();

            RplFundingCalculationServiceMock = new Mock<IRplFundingCalculationService>();
            RplFundingCalculationServiceMock.Setup(x => x.GetRplFundingCalculations
                                                    (It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                                                        It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DbSet<StandardFundingPeriod>>(), 
                                                        It.IsAny<DbSet<FrameworkFundingPeriod>>())).ReturnsAsync(RplFundingCalculation);

            Handler = new GetCohortPriorLearningErrorQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), RplFundingCalculationServiceMock.Object);

            PriorLearning = new ApprenticeshipPriorLearning { DurationReducedBy = 10, PriceReducedBy = 999, DurationReducedByHours = 9 };
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

            TrainingProgramme trainingProgramme = null;
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

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}