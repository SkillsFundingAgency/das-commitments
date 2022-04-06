using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.ApprenticeshipStatusSummaryTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ApprenticeshipStatusSummaryServiceTests
    {
        private ApprenticeshipStatusSummaryServiceTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipStatusSummaryServiceTestFixture();
        }        

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Completed)]
        [TestCase(PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Withdrawn)]
        public async Task ApprenticeshipStatusSummary(PaymentStatus paymentStatus)
        {
            //Arrange          
            _fixture.AddApprenticeship(222, paymentStatus);            

            //Act
            var response = await _fixture.GetResponse(222);

            //Assert            
            Assert.IsNotNull(response);
            Assert.AreEqual(_fixture.LegalEntityIdentifier, response.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().LegalEntityIdentifier);
            Assert.AreEqual(_fixture.organisationType, response.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().LegalEntityOrganisationType);
            if (paymentStatus == PaymentStatus.Active)
                Assert.AreEqual(1, response.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().ActiveCount);
            if (paymentStatus == PaymentStatus.Completed)
                Assert.AreEqual(1, response.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().CompletedCount);
            if (paymentStatus == PaymentStatus.Paused)
                Assert.AreEqual(1, response.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().PausedCount);
            if (paymentStatus == PaymentStatus.Withdrawn)
                Assert.AreEqual(1, response.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().WithdrawnCount);
        }
    }
    
    public class ApprenticeshipStatusSummaryServiceTestFixture
    {
        private Fixture _autoFixture;       

        public string LegalEntityIdentifier { get; }
        public OrganisationType organisationType { get; }       
        public List<Apprenticeship> SeedApprenticeships { get; }
        public ProviderCommitmentsDbContext Db { get; set; }

        public ApprenticeshipStatusSummaryServiceTestFixture()
        {
            LegalEntityIdentifier = "SC171417";
            organisationType = OrganisationType.CompaniesHouse;
            SeedApprenticeships = new List<Apprenticeship>();
            _autoFixture = new Fixture();      
            _autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
              .UseInMemoryDatabase(Guid.NewGuid().ToString())
              .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
              .Options);
        }

        public Task<GetApprenticeshipStatusSummaryQueryResults> GetResponse(long accountId)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var service = new ApprenticeshipStatusSummaryService(lazy, Mock.Of<ILogger<ApprenticeshipStatusSummaryService>>());

                return service.GetApprenticeshipStatusSummary(accountId, CancellationToken.None);
            });
        }

        public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var dbContext = new ProviderCommitmentsDbContext(options))
            {
                dbContext.Database.EnsureCreated();
                SeedData(dbContext);
                return action(dbContext);
            }
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Apprenticeships.AddRange(SeedApprenticeships);
            dbContext.SaveChanges(true);
        }

        public ApprenticeshipStatusSummaryServiceTestFixture AddApprenticeship(long accountId, PaymentStatus paymentStatus)
        {
            var accountLegalEntity = new AccountLegalEntity()
               .Set(a => a.LegalEntityId, LegalEntityIdentifier)
               .Set(a => a.OrganisationType, OrganisationType.CompaniesHouse)
               .Set(a => a.AccountId , accountId)
               .Set(a => a.Id, 444);

            var cohort = new Cohort()
              .Set(c => c.Id, 111)
              .Set(c => c.EmployerAccountId, accountId)
              .Set(c => c.ProviderId, 333)
              .Set(c => c.AccountLegalEntity, accountLegalEntity);

            var apprenticeship = _autoFixture.Build<Apprenticeship>()
             .With(s => s.Cohort, cohort)
             .With(s => s.PaymentStatus, paymentStatus)
             .With(s => s.EndDate, DateTime.UtcNow.AddYears(1))
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .Without(s => s.DataLockStatus)
             .Without(s => s.EpaOrg)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.Continuation)
             .Without(s => s.PreviousApprenticeship)
             .Without(s => s.CompletionDate)
             .Without(s => s.EmailAddressConfirmed)
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Create();

            SeedApprenticeships.Add(apprenticeship);
            return this;
        }       
    }
}
