using AutoFixture;
using AutoFixture.Kernel;
using CsvHelper;
using FluentAssertions;
using FluentAssertions.Equivalency;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
using Xunit.Sdk;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class PriorLearningDataHandlerTests
    {
        PriorLearningDataHandlerTestsFixture fixture;

        [TestCase(100)]
        [TestCase(49)]
        public async Task Handle_WhenCostBeforeRplAreNegative(int reducedByValue)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.CostBeforeRpl = 10;
            fixture.Command.PriceReducedBy = reducedByValue;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => e.PropertyName == "costBeforeRpl" && e.ErrorMessage == "RPL total reduced price should be less than the total price for the apprenticeship").Should().Be(true);
 
        }

        [TestCase(100)]
        [TestCase(49)]
        public async Task Handle_WhenTrainingTotalHoursAreNegative(int reducedByValue)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.TrainingTotalHours = 10;
            fixture.Command.DurationReducedByHours = reducedByValue;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => e.PropertyName == "durationReducedByHours" && e.ErrorMessage == "RPL reduced hours should be less than total course hrs").Should().Be(true);
        }

        [Test]
        public async Task Handle_WhenNoDurationIsSet_ExceptionIsThrown()
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.DurationReducedBy = null;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => e.PropertyName == "durationReducedBy" && e.ErrorMessage == "You must enter the weeks, the weeks can't be negative, the weeks must be 200 or less").Should().Be(true);
        }

        [Test]
        public async Task Handle_WhenNoPriceIsSet_ExceptionIsThrown()
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.PriceReducedBy = null;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => e.PropertyName == "priceReduced" && e.ErrorMessage == "You must enter the price, the price can't be negative, the price must be 100,000 or less").Should().Be(true);
        }

        [TestCase(-1)]
        [TestCase(100001)]
        public async Task Handle_WhenPriceIsSetOutOfValidRange_ExceptionIsThrown(int newPrice)
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.PriceReducedBy = newPrice;
            await fixture.Handle();

            if (newPrice < 0)
            {
                var exception = fixture.Exception as DomainException;
                var domainErrors = exception.DomainErrors.ToList();

                domainErrors.Count().Should().BeGreaterThan(0);
                domainErrors.Any(e => e.PropertyName == "priceReduced" && e.ErrorMessage == "The price can't be negative").Should().Be(true);
            }

            if (newPrice > 100000)
            {
                var exception = fixture.Exception as DomainException;
                var domainErrors = exception.DomainErrors.ToList();

                domainErrors.Count().Should().BeGreaterThan(1);
                domainErrors.Any(e => e.PropertyName == "priceReduced" && e.ErrorMessage == "The price must be 100,000 or less").Should().Be(true);
                domainErrors.Any(e => e.PropertyName == "costBeforeRpl" && e.ErrorMessage == "RPL total reduced price should be less than the total price for the apprenticeship").Should().Be(true);
            }
        }

        [TestCase(null)]
        [TestCase(false)]
        public async Task Handle_WhenRecognisePriorLearningIsNotTrue_ExceptionIsThrown(bool? value)
        {
            fixture = await new PriorLearningDataHandlerTestsFixture().WithRecognisePriorLearningSetTo(value);
            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }

        [Test]
        public async Task Handle_WhenNoDurationReducedByRplIsSet_ExceptionIsThrown()
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.IsDurationReducedByRpl = null;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);

            domainErrors.Any(e => e.PropertyName == "isDurationReducedByRpl" && e.ErrorMessage == "Please select Yes or No").Should().Be(true);
            domainErrors.Any(e => e.PropertyName == "costBeforeRpl" && e.ErrorMessage == "RPL total reduced price should be less than the total price for the apprenticeship").Should().Be(true);

        }

        [Test]
        public async Task Handle_WhenNoCostBeforeRplIsSet_ExceptionIsThrown()
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.CostBeforeRpl = null;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => e.PropertyName == "costBeforeRpl" && e.ErrorMessage == "You must enter the price, the price can't be negative, the price must be 35000 or less").Should().Be(true);
        }

        [Test]
        public async Task Handle_WhenNoDurationReducedByHoursIsSet_ExceptionIsThrown()
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.DurationReducedByHours = null;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => e.PropertyName == "DurationReducedByHours" && e.ErrorMessage == "You must enter the hours, the hours can't be negative, the hours must be 999 or less").Should().Be(true);
        }

        [Test]
        public async Task Handle_WhenNoTrainingTotalHoursIsSet_ExceptionIsThrown()
        {
            fixture = new PriorLearningDataHandlerTestsFixture();
            fixture.Command.TrainingTotalHours = null;
            await fixture.Handle();

            var exception = fixture.Exception as DomainException;
            var domainErrors = exception.DomainErrors.ToList();

            domainErrors.Count().Should().BeGreaterThan(0);
            domainErrors.Any(e => e.PropertyName == "trainingTotalHours" && e.ErrorMessage == "You must enter the hours, the hours can't be negative, the hours must be 9999 or less").Should().Be(true);
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
    }
}