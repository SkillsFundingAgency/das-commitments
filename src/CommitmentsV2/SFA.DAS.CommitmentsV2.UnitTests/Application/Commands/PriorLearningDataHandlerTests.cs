using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningData;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Extensions.AssertExtensions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class PriorLearningDataHandlerTests
    {
        PriorLearningDataHandlerTestsFixture fixture;

        [Test]
        public async Task Handle_WhenAllFieldsAreBlank_Then_AcceptNullData()
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            await fixture.Handle();

            fixture.Exception.ShouldNotBeNull();
        }

        [TestCase(277, "Total off-the-job training time for this apprenticeship standard must be 278 hours or more")]
        [TestCase(0, "Total off-the-job training time for this apprenticeship standard must be 278 hours or more")]
        [TestCase(-10, "Total off-the-job training time for this apprenticeship standard must be 278 hours or more")]
        [TestCase(10000, "Total off-the-job training time for this apprenticeship standard must be 9,999 hours or less")]
        public async Task Handle_WhenTrainingTotalHoursAreInvalid(int trainingTotalHours, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.TrainingTotalHours = trainingTotalHours;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.PropertyName == "trainingTotalHours" &&
                e.ErrorMessage == error).Should().Be(true);
        }

        [TestCase(0, "Total reduction in off-the-job training time due to RPL must be a number between 1 and 9999")]
        [TestCase(-10, "Total reduction in off-the-job training time due to RPL must be a number between 1 and 9999")]
        [TestCase(10000, "Total reduction in off-the-job training time due to RPL must be 9999 hours or less")]
        public async Task Handle_WhenDurationReducedByHoursAreInvalid(int durationReducedByHours, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.DurationReducedByHours = durationReducedByHours;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.PropertyName == "DurationReducedByHours" &&
                e.ErrorMessage == error).Should().Be(true);
        }

        [TestCase(900, 1200, "Total reduction in off-the-job training time due to RPL must be lower than the total off-the-job training time for this apprenticeship standard")]
        [TestCase(900, 700, "The remaining off-the-job training is below the minimum 278 hours required for funding. Check if the RPL reduction is too high")]
        public async Task Handle_WhenTrainTotalHoursWithDurationReducedByHoursAreInvalid(int trainingTotalHours, int durationReducedByHours, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.TrainingTotalHours = trainingTotalHours;
            fixture.Command.DurationReducedByHours = durationReducedByHours;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.PropertyName == "DurationReducedByHours" &&
                e.ErrorMessage == error).Should().Be(true);
        }

        [TestCase(0, "Reduction in duration must be 1 week or more")]
        [TestCase(-1, "Reduction in duration must be 1 week or more")]
        [TestCase(261, "Reduction in duration must be 260 weeks or less")]
        public async Task Handle_WhenDurationReducedIsSetIncorrectly_ExceptionIsThrown(int? durationReducedBy, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.IsDurationReducedByRpl = true;
            fixture.Command.DurationReducedBy = durationReducedBy;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => 
            e.PropertyName == "durationReducedBy" && 
            e.ErrorMessage == error).Should().Be(true);
        }

        [TestCase(261, "Reduction in duration should not have a value")]
        public async Task Handle_WhenDurationReducedIsNotExpectedToBeSet_ExceptionIsThrown(int? durationReducedBy, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.IsDurationReducedByRpl = false;
            fixture.Command.DurationReducedBy = durationReducedBy;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.PropertyName == "isDurationReducedByRpl" &&
                e.ErrorMessage == error).Should().Be(true);
        }

        [TestCase(4, "Total price reduction due to RPL must be 5 pounds or more")]
        [TestCase(-1, "Total price reduction due to RPL must be 5 pounds or more")]
        [TestCase(18001, "Total price reduction due to RPL must be 18,000 or less")]
        public async Task Handle_WhenPriceReducedIsInvalid_ExceptionIsThrown(int? priceReducedBy, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.PriceReducedBy = priceReducedBy;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => 
            e.PropertyName == "priceReduced" && 
            e.ErrorMessage == error).Should().Be(true);
        }

        [TestCase(null)]
        [TestCase(false)]
        public async Task Handle_WhenRecognisePriorLearningIsNotTrue_ExceptionIsThrown(bool? value)
        {
            fixture = await new PriorLearningDataHandlerTestsFixture().WithRecognisePriorLearningSetTo(value);
            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }

        [TestCase(true, 10)]
        [TestCase(false, null)]
        [Test]
        public async Task Handle_WhenRplData_is_Valid(bool isDurationReducedByRpl, int? durationReducedBy)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.TrainingTotalHours = 3000;
            fixture.Command.DurationReducedByHours = 180;
            fixture.Command.IsDurationReducedByRpl = isDurationReducedByRpl;
            fixture.Command.DurationReducedBy = durationReducedBy;
            fixture.Command.PriceReducedBy = 100;

            await fixture.Handle();

            fixture.VerifyRplDataMatchesCommand();
        }
    }

    public class PriorLearningDataHandlerTestsFixture
    {
        public long ApprenticeshipId = 12;
        public Fixture fixture { get; set; }
        public PriorLearningDataCommand Command { get; set; }
        public DraftApprenticeship ApprenticeshipDetails { get; set; }
        public Cohort Cohort { get; set; }
        public AccountLegalEntity AccountLegalEntity { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<PriorLearningDataCommand> Handler { get; set; }
        public UserInfo UserInfo { get; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public RplSettingsConfiguration RplSettingsConfiguration { get; set; }
        public DraftApprenticeship DraftApprenticeshipFromDb => 
            Db.DraftApprenticeships.First(x => x.Id == ApprenticeshipId);

        public Exception Exception { get; set; }

        public PriorLearningDataHandlerTestsFixture()
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
                .Set(c=>c.WithParty, Party.Employer)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.AccountLegalEntityId, AccountLegalEntity.Id)
                .Set(c => c.AccountLegalEntity, AccountLegalEntity);

            ApprenticeshipDetails = fixture.Build<DraftApprenticeship>()
             .With(s => s.Id, ApprenticeshipId)
             .With(s => s.Cohort, Cohort)
             .With(s => s.EndDate, DateTime.UtcNow)
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .With(a=>a.RecognisePriorLearning, true)
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.FlexibleEmployment)
             .Without(s => s.PreviousApprenticeship)
             .Create();

            CancellationToken = new CancellationToken();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().EnableSensitiveDataLogging()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            UserInfo = fixture.Create<UserInfo>();
            Command = fixture.Build<PriorLearningDataCommand>()
                .With(o => o.UserInfo, UserInfo)
                .With(o => o.ApprenticeshipId, ApprenticeshipId)
                .With(o => o.CohortId, Cohort.Id)
                .Create();

            RplSettingsConfiguration = new RplSettingsConfiguration {MinimumPriceReduction = 5, MaximumTrainingTimeReduction = 9999};

            Handler = new RecognisePriorLearningDataHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                RplSettingsConfiguration,
                Mock.Of<ILogger<RecognisePriorLearningDataHandler>>());

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

        public async Task<PriorLearningDataHandlerTestsFixture> SeedData()
        {
            Db.DraftApprenticeships.Add(ApprenticeshipDetails);
            await Db.SaveChangesAsync();
            return this;
        }

        public async Task<PriorLearningDataHandlerTestsFixture> WithRecognisePriorLearningSetTo(bool? recognisePriorLearning)
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

        public void VerifyRplDataMatchesCommand()
        {
            var first = Db.DraftApprenticeships.First();
            first.PriorLearning.ShouldNotBeNull();

            first.TrainingTotalHours.Should().Be(Command.TrainingTotalHours);

            first.PriorLearning.DurationReducedByHours.Should().Be(Command.DurationReducedByHours);
            first.PriorLearning.IsDurationReducedByRpl.Should().Be(Command.IsDurationReducedByRpl);
            first.PriorLearning.DurationReducedBy.Should().Be(Command.DurationReducedBy);
            first.PriorLearning.PriceReducedBy.Should().Be(Command.PriceReducedBy);
        }

    }
}