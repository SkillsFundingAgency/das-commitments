using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningData;
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

        [TestCase(100, 10, "RPL total reduced price should be less than the total price for the apprenticeship")]
        [TestCase(49, 10, "RPL total reduced price should be less than the total price for the apprenticeship")]
        public async Task Handle_WhenCostBeforeRplAreNegative(int reducedByValue, int costBeforeRpl, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.CostBeforeRpl = costBeforeRpl;
            fixture.Command.PriceReducedBy = reducedByValue;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => 
            e.PropertyName == "costBeforeRpl" && 
            e.ErrorMessage == error).Should().Be(true);

        }

        [TestCase(100, 10, "RPL reduced hours should be less than total course hrs")]
        [TestCase(49, 10, "RPL reduced hours should be less than total course hrs")]
        public async Task Handle_WhenTrainingTotalHoursAreNegative(int reducedByValue, int trainingTotalHours, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.TrainingTotalHours = 10;
            fixture.Command.DurationReducedByHours = reducedByValue;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e =>
                e.PropertyName == "durationReducedByHours" &&
                e.ErrorMessage == error).Should().Be(true);
        }

        [TestCase(null, "You must enter the weeks, the weeks can't be negative, the weeks must be 200 or less")]
        [TestCase(-1, "The number can't be negative")]
        [TestCase(999, "The weeks entered must be 200 or less")]
        public async Task Handle_WhenNoDurationIsSet_ExceptionIsThrown(int? durationReducedBy, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.DurationReducedBy = durationReducedBy;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => 
            e.PropertyName == "durationReducedBy" && 
            e.ErrorMessage == error).Should().Be(true);
        }

        [TestCase(null, "You must enter the price, the price can't be negative, the price must be 100,000 or less")]
        [TestCase(999999, "The price must be 100,000 or less")]
        [TestCase(-1, "The price can't be negative")]
        public async Task Handle_WhenNoPriceIsSet_ExceptionIsThrown(int? priceReducedBy, string error)
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

        [TestCase(-1, 200000, "The price can't be negative")]
        [TestCase(100001, 200000, "The price must be 100,000 or less")]
        public async Task Handle_WhenPriceIsSetOutOfValidRange_ExceptionIsThrown(int newPrice, int costBeforeRpl, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.PriceReducedBy = newPrice;
            fixture.Command.CostBeforeRpl = costBeforeRpl;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors
                .Any(e => 
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

        [TestCase(null, "Please select Yes or No")]
        public async Task Handle_WhenNoDurationReducedByRplIsSet_ExceptionIsThrown(bool? cval, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.IsDurationReducedByRpl = cval;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);

            domainErrors
                .Any(e => 
                e.PropertyName == "isDurationReducedByRpl" &&
                e.ErrorMessage == error).Should().Be(true);

        }

        [TestCase(-1, "The price can't be negative")]
        [TestCase(100000, "The price entered must be 35000 or less")]
        [TestCase(null, "You must enter the price, the price can't be negative, the price must be 35000 or less")]
        public async Task Handle_WhenNoCostBeforeRplIsSet_ExceptionIsThrown(int? cval, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.CostBeforeRpl = cval;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => 
                e.PropertyName == "costBeforeRpl" && 
                e.ErrorMessage == error).Should().Be(true);
        }


        [TestCase(null, "You must enter the hours, the hours can't be negative, the hours must be 999 or less")]
        [TestCase(-1, "The hours can't be negative")]
        [TestCase(100000, "The hours entered must be 999 or less")]
        public async Task Handle_WhenNoDurationReducedByHoursIsSet_ExceptionIsThrown(int? cval,  string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.DurationReducedByHours = cval;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => 
            e.PropertyName == "DurationReducedByHours" && e.ErrorMessage == error).Should()
                .Be(true);
        }

        [TestCase(null, "You must enter the hours, the hours can't be negative, the hours must be 9999 or less")]
        [TestCase(-1, "The hours can't be negative")]
        [TestCase(99999, "The hours entered must be 9999 or less")]
        public async Task Handle_WhenNoTrainingTotalHoursIsSet_ExceptionIsThrown(int? cval, string error)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.TrainingTotalHours = cval;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => e.PropertyName == "trainingTotalHours" && e.ErrorMessage == error).Should().Be(true);
        }


        [TestCase(true, 1)]
        [TestCase(false, null)]
        [Test]
        public async Task Handle_WhenRplData_is_Valid(bool isDurationReducedByRpl, int? durationReducedBy)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.TrainingTotalHours = 100;
            fixture.Command.DurationReducedByHours = 10;
            fixture.Command.IsDurationReducedByRpl = isDurationReducedByRpl;
            fixture.Command.DurationReducedBy = durationReducedBy;
            fixture.Command.CostBeforeRpl = 1000;
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
                .With(o => o.PriceReducedBy, 2000)
                .Create();

            Handler = new RecognisePriorLearningDataHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
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
            Assert.IsNotNull(Exception);
            Assert.IsInstanceOf<T>(Exception);
        }

        public void VerifyRplDataMatchesCommand()
        {
            var first = Db.DraftApprenticeships.First();
            first.PriorLearning.ShouldNotBeNull();

            first.TrainingTotalHours.Should().Be(Command.TrainingTotalHours);
            first.CostBeforeRpl.Should().Be(Command.CostBeforeRpl);

            first.PriorLearning.DurationReducedByHours.Should().Be(Command.DurationReducedByHours);
            first.PriorLearning.IsDurationReducedByRpl.Should().Be(Command.IsDurationReducedByRpl);
            first.PriorLearning.DurationReducedBy.Should().Be(Command.DurationReducedBy);
            first.PriorLearning.PriceReducedBy.Should().Be(Command.PriceReducedBy);
        }

    }
}