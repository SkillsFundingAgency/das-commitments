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
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork;

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
        public async Task CreateCohort_Provider_Creates_Cohort()
        {
            await _fixture.CreateCohort();
            _fixture.VerifyProviderCohortCreation();
        }

        [Test]
        public async Task AddDraftApprenticeship_Provider_Adds_Draft_Apprenticeship()
        {
            _fixture.SetCohort();
            await _fixture.AddDraftApprenticeship();
            _fixture.VerifyProviderDraftApprenticeshipAdded();
        }

        [Test]
        public void AddDraftApprenticeship_CohortNotFound_ShouldThrowException()
        {
            Assert.ThrowsAsync<BadRequestException>(_fixture.AddDraftApprenticeship, $"Cohort {_fixture.CohortId} was not found");
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

        [TestCase(true, false)]
        [TestCase(false, true)]
        public async Task Reservation_Validation(bool hasValidationError, bool passes)
        {
            await _fixture.WithReservationValidationResult(hasValidationError)
                .CreateCohort();

            _fixture.VerifyReservationException(passes);
        }

        [Test]
        public async Task Reservation_Validation_Skipped()
        {
            await _fixture.CreateCohort();
            _fixture.VerifyReservationValidationNotPerformed();
        }

        [Test]
        public async Task OverlapOnStartDate_Validation()
        {
            await _fixture.WithUlnOverlapOnStartDate().CreateCohort();
            _fixture.VerifyOverlapExceptionOnStartDate();
        }

        [Test]
        public async Task OverlapOnEndDate_Validation()
        {
            await _fixture.WithUlnOverlapOnEndDate().CreateCohort();
            _fixture.VerifyOverlapExceptionOnEndDate();
        }

        public class CohortDomainServiceTestFixture
        {
            public CohortDomainService CohortDomainService { get; set; }
            public ProviderCommitmentsDbContext Db { get; set; }
            public long ProviderId { get; }
            public long AccountLegalEntityId { get; }
            public long CohortId { get; }
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; }
            
            public Mock<Provider> Provider { get; set; }
            public Mock<AccountLegalEntity> AccountLegalEntity { get; set; }
            public Cohort Cohort { get; set; }
            public Mock<IAcademicYearDateProvider> AcademicYearDateProvider { get; }
            public Mock<IUlnValidator> UlnValidator { get; }
            public Mock<IReservationValidationService> ReservationValidationService { get; }
            private Mock<IOverlapCheckService> OverlapCheckService { get; }
            public Party Party { get; set; }
            private Mock<IAuthenticationService> AuthenticationService { get; }
            public List<DomainError>  DomainErrors { get; private set; }

            public CohortDomainServiceTestFixture()
            {
                // We need this to allow the UoW to initialise it's internal static events collection.
                var uow = new UnitOfWorkContext();

                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);

                ProviderId = 1;
                AccountLegalEntityId = 2;
                CohortId = 3;

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

                AcademicYearDateProvider = new Mock<IAcademicYearDateProvider>();
                AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(new DateTime(2020, 7, 31));

                UlnValidator = new Mock<IUlnValidator>();
                UlnValidator.Setup(x => x.Validate(It.IsAny<string>())).Returns(UlnValidationResult.Success);

                ReservationValidationService = new Mock<IReservationValidationService>();
                ReservationValidationService.Setup(x =>
                        x.Validate(It.IsAny<ReservationValidationRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => new ReservationValidationResult(new ReservationValidationError[0]));

                OverlapCheckService = new Mock<IOverlapCheckService>();
                OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()));

                Party = Party.Provider;
                AuthenticationService = new Mock<IAuthenticationService>();
                AuthenticationService.Setup(x => x.GetUserParty()).Returns(Party);
                
                DomainErrors = new List<DomainError>();

                CohortDomainService = new CohortDomainService(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                    Mock.Of<ILogger<CohortDomainService>>(),
                    AcademicYearDateProvider.Object,
                    UlnValidator.Object,
                    ReservationValidationService.Object,
                    OverlapCheckService.Object,
                    AuthenticationService.Object
                    );
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

            public CohortDomainServiceTestFixture WithReservationValidationResult(bool hasReservationError)
            {
                DraftApprenticeshipDetails.ReservationId = Guid.NewGuid();
                DraftApprenticeshipDetails.StartDate = new DateTime(2019, 01, 01);
                DraftApprenticeshipDetails.TrainingProgramme = new TrainingProgramme("TEST",
                    "TEST",
                    ProgrammeType.Standard,
                    new DateTime(2016, 1, 1),
                    null);

                var errors = new List<ReservationValidationError>();

                if (hasReservationError)
                {
                    errors.Add(new ReservationValidationError("TEST", "TEST"));
                }

                ReservationValidationService.Setup(x => x.Validate(It.IsAny<ReservationValidationRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => new ReservationValidationResult(errors.ToArray()));

                return this;
            }

            public CohortDomainServiceTestFixture WithUlnOverlapOnStartDate()
            {
                DraftApprenticeshipDetails.Uln = "X";
                DraftApprenticeshipDetails.StartDate = new DateTime(2020, 1, 1);
                DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);
                
                OverlapCheckService.Setup(x => x.CheckForOverlaps(It.Is<string>(uln => uln == "X"), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => new OverlapCheckResult(true, false));

                return this;
            }

            public CohortDomainServiceTestFixture WithUlnOverlapOnEndDate()
            {
                DraftApprenticeshipDetails.Uln = "X";
                DraftApprenticeshipDetails.StartDate = new DateTime(2020, 1, 1);
                DraftApprenticeshipDetails.EndDate = new DateTime(2021, 1, 1);

                OverlapCheckService.Setup(x => x.CheckForOverlaps(It.Is<string>(uln => uln == "X"), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => new OverlapCheckResult(false, true));

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

            public CohortDomainServiceTestFixture SetCohort()
            {
                Cohort = new Cohort
                {
                    Id = CohortId,
                    EditStatus = EditStatus.EmployerOnly
                };
                Db.Commitment.Add(Cohort);

                return this;
            }
            
            public async Task<Cohort> CreateCohort()
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

            public async Task AddDraftApprenticeship()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    await CohortDomainService.AddDraftApprenticeship(ProviderId, CohortId, DraftApprenticeshipDetails, new CancellationToken());
                    await Db.SaveChangesAsync();
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                }
            }

            public void VerifyProviderCohortCreation()
            {
                Provider.Verify(x => x.CreateCohort(Provider.Object, AccountLegalEntity.Object, DraftApprenticeshipDetails, Party));
            }

            public void VerifyProviderDraftApprenticeshipAdded()
            {
                //Cohort.Verify(x => x.AddDraftApprenticeship(DraftApprenticeshipDetails, Party));



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

            public void VerifyReservationException(bool passes)
            {
                if (passes)
                {
                    Assert.IsFalse(EnumerableExtensions.Any(DomainErrors));
                    return;
                }

                Assert.IsTrue(DomainErrors.Any(x => x.PropertyName == "TEST"));
            }

            public void VerifyReservationValidationNotPerformed()
            {
               ReservationValidationService.Verify(x => x.Validate(It.IsAny<ReservationValidationRequest>(),
                       It.IsAny<CancellationToken>()),
                   Times.Never);
            }

            public void VerifyOverlapExceptionOnStartDate()
            {
                Assert.IsTrue(DomainErrors.Any(x => x.PropertyName == "StartDate"));
            }
            public void VerifyOverlapExceptionOnEndDate()
            {
                Assert.IsTrue(DomainErrors.Any(x => x.PropertyName == "EndDate"));
            }
        } 
    }
}
