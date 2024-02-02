using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningDetails;
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
    public class PriorLearningDetailsHandlerTests
    {
        [Test]
        public async Task Handle_WhenCommandIsHandled_PriorLearningDetailsAreUpdated()
        { 
            using var fixture = new PriorLearningDetailsHandlerTestsFixture();
            await fixture.Handle();

            Assert.That(fixture.DraftApprenticeshipFromDb.PriorLearning.DurationReducedBy, Is.EqualTo(fixture.Command.DurationReducedBy));
            Assert.That(fixture.DraftApprenticeshipFromDb.PriorLearning.PriceReducedBy, Is.EqualTo(fixture.Command.PriceReducedBy));
        }

        [Test]
        public async Task Handle_WhenNoDurationIsSet_ExceptionIsThrown()
        {
            using var fixture = new PriorLearningDetailsHandlerTestsFixture();
            fixture.Command.DurationReducedBy = null;
            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }

        [TestCase(-1)]
        [TestCase(1000)]
        public async Task Handle_WhenDurationIsSetOutsideValidRange_ExceptionIsThrown(int newDuration)
        {
            using var fixture = new PriorLearningDetailsHandlerTestsFixture();
            fixture.Command.DurationReducedBy = newDuration;
            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }

        [Test]
        public async Task Handle_WhenNoPriceIsSet_ExceptionIsThrown()
        {
            using var fixture = new PriorLearningDetailsHandlerTestsFixture();
            fixture.Command.PriceReducedBy = null;
            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }

        [TestCase(-1)]
        [TestCase(100001)]
        public async Task Handle_WhenPriceIsSetOutOfValidRange_ExceptionIsThrown(int newPrice)
        {
            using var fixture = new PriorLearningDetailsHandlerTestsFixture();
            fixture.Command.PriceReducedBy = newPrice;
            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }

        [TestCase(null)]
        [TestCase(false)]
        public async Task Handle_WhenRecognisePriorLearningIsNotTrue_ExceptionIsThrown(bool? value)
        {
            using var fixture = await new PriorLearningDetailsHandlerTestsFixture().WithRecognisePriorLearningSetTo(value);
            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }
    }

    public class PriorLearningDetailsHandlerTestsFixture : IDisposable
    {
        public long ApprenticeshipId = 12;
        public Fixture fixture { get; set; }
        public PriorLearningDetailsCommand Command { get; set; }
        public DraftApprenticeship ApprenticeshipDetails { get; set; }
        public Cohort Cohort { get; set; }
        public AccountLegalEntity AccountLegalEntity { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<PriorLearningDetailsCommand> Handler { get; set; }
        public UserInfo UserInfo { get; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public DraftApprenticeship DraftApprenticeshipFromDb =>
            Db.DraftApprenticeships.First(x => x.Id == ApprenticeshipId);

        public Exception Exception { get; set; }

        public PriorLearningDetailsHandlerTestsFixture()
        {
            fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(SFA.DAS.CommitmentsV2.Models.ApprenticeshipBase),
                    typeof(DraftApprenticeship)));

            UnitOfWorkContext = new UnitOfWorkContext();

            AccountLegalEntity = new AccountLegalEntity()
                .Set(c => c.Id, 555)
                .Set(c => c.AccountId, 444);

            Cohort = new CommitmentsV2.Models.Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.WithParty, Party.Employer)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.AccountLegalEntityId, AccountLegalEntity.Id)
                .Set(c => c.AccountLegalEntity, AccountLegalEntity);

            ApprenticeshipDetails = fixture.Build<DraftApprenticeship>()
             .With(s => s.Id, ApprenticeshipId)
             .With(s => s.Cohort, Cohort)
             .With(s => s.EndDate, DateTime.UtcNow)
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .With(a => a.RecognisePriorLearning, true)
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.FlexibleEmployment)
             .Without(s => s.PreviousApprenticeship)
             .Create();

            CancellationToken = new CancellationToken();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().EnableSensitiveDataLogging()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            UserInfo = fixture.Create<UserInfo>();
            Command = fixture.Build<PriorLearningDetailsCommand>()
                .With(o => o.UserInfo, UserInfo)
                .With(o=> o.Rpl2Mode, false)
                .With(o=> o.ApprenticeshipId, ApprenticeshipId)
                .With(o=> o.CohortId, Cohort.Id)
                .Create();

            Handler = new PriorLearningDetailsHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                Mock.Of<ILogger<PriorLearningDetailsHandler>>());

            var _ = SeedData().Result;
        }

        public async Task Handle()
        {
            try
            {
                await Handler.Handle(Command, CancellationToken);
            }
            catch (Exception exception)
            {
                Exception = exception;
            }
        }

        private async Task<PriorLearningDetailsHandlerTestsFixture> SeedData()
        {
            Db.DraftApprenticeships.Add(ApprenticeshipDetails);
            await Db.SaveChangesAsync();
            return this;
        }

        public async Task<PriorLearningDetailsHandlerTestsFixture> WithRecognisePriorLearningSetTo(bool? recognisePriorLearning)
        {
            var apprenticeship = Db.DraftApprenticeships.First();
            apprenticeship.RecognisePriorLearning = recognisePriorLearning;
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
        }
    }
}