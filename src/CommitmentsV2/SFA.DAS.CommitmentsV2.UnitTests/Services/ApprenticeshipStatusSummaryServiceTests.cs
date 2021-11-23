using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.UnitTests.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    public class ApprenticeshipStatusSummaryServiceTests
    {
        private Fixture _autoFixture;
        private ApprenticeshipStatusSummaryService _Sut;
        private GetApprenticeshipStatusSummaryQueryResults _result;
        private long EmployerAccountId;        
        public Mock<ILogger<ApprenticeshipStatusSummaryService>> Logger { get; set; }

        [SetUp]
        public void Arrange()
        {
            _autoFixture = new Fixture();
        }

        [Test]
        public async Task TestHandler()
        {
            //Arrange            
            var apprenticeshipStatusSummaries = new SpAsyncEnumerableQueryable<ApprenticeshipStatusSummary>(new ApprenticeshipStatusSummary()
            {
                LegalEntityId = "SC171417",
                LegalEntityOrganisationType = OrganisationType.CompaniesHouse,
                PaymentStatus = Types.PaymentStatus.Paused,
                Count = 1
            },
            new ApprenticeshipStatusSummary()
            {
                LegalEntityId = "SC171417",
                LegalEntityOrganisationType = OrganisationType.CompaniesHouse,
                PaymentStatus = Types.PaymentStatus.Active,
                Count = 2402
            });

            //https://nodogmablog.bryanhogan.net/2017/11/unit-testing-entity-framework-core-stored-procedures/           
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            ProviderCommitmentsDbContext context = new ProviderCommitmentsDbContext(options);
            context.ApprenticeshipStatusSummary = context.ApprenticeshipStatusSummary.MockFromSql(apprenticeshipStatusSummaries);
            Logger = new Mock<ILogger<ApprenticeshipStatusSummaryService>>();
            _Sut = new ApprenticeshipStatusSummaryService(new Lazy<ProviderCommitmentsDbContext>(() => context), Logger.Object);
            EmployerAccountId = _autoFixture.Create<long>();

            //Act
            _result = await _Sut.GetApprenticeshipStatusSummary(EmployerAccountId, default);

            //Assert
            Assert.IsNotNull(_result);
            Assert.AreEqual(_result.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().LegalEntityIdentifier, "SC171417");
            Assert.AreEqual(_result.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().LegalEntityOrganisationType, Api.Types.Responses.OrganisationType.CompaniesHouse);
            Assert.AreEqual(_result.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().ActiveCount, 2402);
            Assert.AreEqual(_result.GetApprenticeshipStatusSummaryQueryResult.FirstOrDefault().PausedCount, 1);            
        }
    }   
}
