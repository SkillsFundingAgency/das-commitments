using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class CohortDomainServiceTests
    {
        private CohortDomainServiceTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortDomainServiceTestFixture();
        }

        [Test]
        public async Task OnCreateCohort_Provider_Creates_Cohort()
        {
            await _fixture.CreateCohort();
            _fixture.VerifyProviderCohortCreation();
        }

        [TestCase("2019-07-31", null, true)]
        [TestCase("2019-07-31", "2020-08-01", false, Description = "One day after cut off")]
        [TestCase("2019-07-31", "2020-07-31", true, Description = "Day of cut off (last valid day)")]
        [TestCase("2019-07-31", "2018-01-01", true, Description = "Day in the past")]
        public async Task StartDate_CheckIsWithinAYearOfEndOfCurrentTeachingYear_Validation(DateTime academicYearEndDate, DateTime? startDate, bool passes)
        {
            await _fixture
                .WithAcademicYearEndDate(academicYearEndDate)
                .WithStartDate(startDate)
                .CreateCohort();

            _fixture.VerifyStartDateException(passes);
        }

        [TestCase(UlnValidationResult.IsEmptyUlnNumber, true)]
        [TestCase(UlnValidationResult.Success, true)]
        [TestCase(UlnValidationResult.IsInValidTenDigitUlnNumber, false)]
        [TestCase(UlnValidationResult.IsInvalidUln, false)]
        public async Task Uln_Validation(UlnValidationResult validationResult, bool passes)
        {
            await _fixture
                .WithUlnValidationResult(validationResult)
                .CreateCohort();

            _fixture.VerifyUlnException(passes);
        }

        public class CohortDomainServiceTestFixture
        {
            public CohortDomainService CohortDomainService { get; set; }
            public ProviderCommitmentsDbContext Db { get; set; }
            public long ProviderId { get; }
            public long AccountLegalEntityId { get; }
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; }
            
            public Mock<Provider> Provider { get; set; }
            public Mock<AccountLegalEntity> AccountLegalEntity { get; set; }            
            public Mock<ICurrentDateTime> CurrentDateTime { get; private set; }
            public Mock<IAcademicYearDateProvider> AcademicYearDateProvider { get; private set; }
            public Mock<IUlnValidator> UlnValidator { get; private set; }
            public List<DomainError>  DomainErrors { get; private set; }

            public CohortDomainServiceTestFixture()
            {
                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);

                ProviderId = 1;
                AccountLegalEntityId = 2;

                Provider = new Mock<Provider>();
                Provider.Setup(x => x.UkPrn).Returns(ProviderId);
                Db.Providers.Add(Provider.Object);

                AccountLegalEntity = new Mock<AccountLegalEntity>();
                AccountLegalEntity.Setup(x => x.Id).Returns(AccountLegalEntityId);
                Db.AccountLegalEntities.Add(AccountLegalEntity.Object);
                
                DraftApprenticeshipDetails = new DraftApprenticeshipDetails
                {
                    FirstName = "Test", LastName = "Test"
                };               

                CurrentDateTime = new Mock<ICurrentDateTime>();
                CurrentDateTime.Setup(x => x.UtcNow).Returns(new DateTime(2018, 1, 1));

                AcademicYearDateProvider = new Mock<IAcademicYearDateProvider>();
                AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(new DateTime(2020, 7, 31));

                UlnValidator = new Mock<IUlnValidator>();
                UlnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(UlnValidationResult.Success);

                DomainErrors = new List<DomainError>();

                CohortDomainService = new CohortDomainService(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                    CurrentDateTime.Object,
                    Mock.Of<ILogger<CohortDomainService>>(),
                    AcademicYearDateProvider.Object,
                    UlnValidator.Object);
            }

            public CohortDomainServiceTestFixture WithCurrentDate(DateTime value)
            {
                var utcValue = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                CurrentDateTime.Setup(x => x.UtcNow).Returns(utcValue);
                return this;
            }

            public CohortDomainServiceTestFixture WithAcademicYearEndDate(DateTime value)
            {
                var utcValue = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(utcValue);
                return this;
            }

            public CohortDomainServiceTestFixture WithUlnValidationResult(UlnValidationResult value)
            {
                DraftApprenticeshipDetails.Uln = "X";
                UlnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(value);
                return this;
            }

            public CohortDomainServiceTestFixture WithStartDate(DateTime? startDate)
            {
                var utcStartDate = startDate.HasValue
                    ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                    : default(DateTime?);

                DraftApprenticeshipDetails.StartDate = utcStartDate;
                return this;
            }

            public async Task<Commitment> CreateCohort()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    var result = await CohortDomainService.CreateCohort(ProviderId, AccountLegalEntityId, DraftApprenticeshipDetails, new CancellationToken());
                    await Db.SaveChangesAsync();
                    return result;
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                    return null;
                }
            }

            public void VerifyProviderCohortCreation()
            {
                Provider.Verify(x => x.CreateCohort(It.Is<AccountLegalEntity>(ale => ale == AccountLegalEntity.Object), It.IsAny<DraftApprenticeshipDetails>()));
            }

            public void VerifyStartDateException(bool passes)
            {
                if (passes)
                {
                    Assert.IsFalse(EnumerableExtensions.Any(DomainErrors));
                    return;
                }

                Assert.IsTrue(DomainErrors.Any(x => x.PropertyName == "StartDate"));
            }

            public void VerifyUlnException(bool passes)
            {
                if (passes)
                {
                    Assert.IsFalse(EnumerableExtensions.Any(DomainErrors));
                    return;
                }

                Assert.IsTrue(DomainErrors.Any(x => x.PropertyName == "Uln"));
            }
        } 
    }
}
