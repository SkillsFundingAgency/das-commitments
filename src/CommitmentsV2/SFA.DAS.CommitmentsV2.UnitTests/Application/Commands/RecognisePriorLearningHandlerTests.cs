using AutoFixture.Kernel;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.RecognisePriorLearning;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class RecognisePriorLearningHandlerTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public async Task Handle_WhenCommandIsHandled_RecognisePriorLearningIsUpdated(bool expected)
        {
            using var fixture = new RecognisePriorLearningHandlerTestsFixture();
            fixture.Command.RecognisePriorLearning = expected;

            await fixture.Handle();

            Assert.That(fixture.DraftApprenticeshipFromDb.RecognisePriorLearning, Is.EqualTo(expected));
        }

        [Test]
        public async Task Handle_WhenNoRecognisePriorLearningIsSet_ExceptionIsThrown()
        {
            using var fixture = new RecognisePriorLearningHandlerTestsFixture();
            fixture.Command.RecognisePriorLearning = null;
            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }

        [Test]
        public async Task Handle_WhenPriorLearningExistsAndRecognisedPriorLearningIsFalse_PriorLearningDetailsAreNulled()
        {
            using var fixture = await new RecognisePriorLearningHandlerTestsFixture().WithPriorLearningDetailsSet();
            fixture.Command.RecognisePriorLearning = false;

            await fixture.Handle();

            Assert.Multiple(() =>
            {
                Assert.That(fixture.DraftApprenticeshipFromDb.PriorLearning?.DurationReducedBy, Is.Null);
                Assert.That(fixture.DraftApprenticeshipFromDb.PriorLearning?.PriceReducedBy, Is.Null);
                Assert.That(fixture.DraftApprenticeshipFromDb.PriorLearning?.DurationReducedByHours, Is.Null);
            });
        }
    }

    public class RecognisePriorLearningHandlerTestsFixture : IDisposable
    {
        public long ApprenticeshipId = 12;
        public Fixture Fixture { get; set; }
        public RecognisePriorLearningCommand Command { get; set; }
        public DraftApprenticeship ApprenticeshipDetails { get; set; }
        public Cohort Cohort { get; set; }
        public AccountLegalEntity AccountLegalEntity { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<RecognisePriorLearningCommand> Handler { get; set; }
        public UserInfo UserInfo { get; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public ApprenticeshipPriorLearning PriorLearning { get; set; }

        public DraftApprenticeship DraftApprenticeshipFromDb => 
            Db.DraftApprenticeships.First(x => x.Id == ApprenticeshipId);

        public Exception Exception { get; set; }

        public RecognisePriorLearningHandlerTestsFixture()
        {
            Fixture = new Fixture();
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            Fixture.Customizations.Add(
                new TypeRelay(
                    typeof(ApprenticeshipBase),
                    typeof(DraftApprenticeship)));

            UnitOfWorkContext = new UnitOfWorkContext();

            AccountLegalEntity = new AccountLegalEntity()
                .Set(c => c.Id, 555)
                .Set(c => c.Address, "High street")
                .Set(c => c.LegalEntityId, "SOMEID")
                .Set(c => c.PublicHashedId, "XXXX")
                .Set(c => c.Name, "Name")
                .Set(c => c.AccountId, 444);

            Cohort = new Cohort()
                .Set(c => c.Reference, Guid.NewGuid().ToString())
                .Set(c => c.Id, 111)
                .Set(c => c.Reference, "XXXX")
                .Set(c => c.EmployerAccountId, 222)
                .Set(c=>c.WithParty, Party.Employer)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.AccountLegalEntityId, AccountLegalEntity.Id)
                .Set(c => c.AccountLegalEntity, AccountLegalEntity);

            ApprenticeshipDetails = Fixture.Build<DraftApprenticeship>()
             .With(s => s.Id, ApprenticeshipId)
             .With(s => s.Cohort, Cohort)
             .With(s => s.EndDate, DateTime.UtcNow)
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.FlexibleEmployment)
             .Without(s => s.PreviousApprenticeship)
             .Without(s=>s.PriorLearning)
             .Create();

            PriorLearning = new ApprenticeshipPriorLearning { DurationReducedBy = 9, PriceReducedBy = 190, DurationReducedByHours = 900, IsDurationReducedByRpl = true};

            CancellationToken = new CancellationToken();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().EnableSensitiveDataLogging()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            UserInfo = Fixture.Create<UserInfo>();
            Command = Fixture.Build<RecognisePriorLearningCommand>().With(o => o.UserInfo, UserInfo).Create();
            Command.ApprenticeshipId = ApprenticeshipId;
            Command.CohortId = Cohort.Id;

            Handler = new RecognisePriorLearningHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                Mock.Of<ILogger<RecognisePriorLearningHandler>>());

            var _ = SeedData().Result;
        }

        public async Task Handle()
        {
            try
            {
                await Handler.Handle(Command, CancellationToken);
            }
            catch(Exception exception)
            {
                Exception = exception;
            }
        }

        public async Task<RecognisePriorLearningHandlerTestsFixture> SeedData()
        {
            Db.DraftApprenticeships.Add(ApprenticeshipDetails);
            await Db.SaveChangesAsync();
            return this;
        }

        public async Task<RecognisePriorLearningHandlerTestsFixture> WithPriorLearningDetailsSet()
        {
            var apprenticeship = Db.DraftApprenticeships.First();
            apprenticeship.RecognisePriorLearning = true;
            apprenticeship.TrainingTotalHours = 1000;
            apprenticeship.PriorLearning = PriorLearning;
            await Db.SaveChangesAsync();
            return this;
        }

        public void VerifyException<T>()
        {
            Assert.That(Exception, Is.Not.Null);
            Assert.That(Exception, Is.InstanceOf<T>());
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}