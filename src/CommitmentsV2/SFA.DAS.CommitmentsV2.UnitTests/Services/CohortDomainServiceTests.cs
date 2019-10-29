using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
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
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

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

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task CreateCohort_CreatingParty_Creates_Cohort(Party party)
        {
            await _fixture
                .WithParty(party)
                .CreateCohort();
            _fixture.VerifyCohortCreation(party);
        }

        [Test]
        public async Task CreateCohortWithOtherParty_Creates_Cohort()
        {
            await _fixture
                .WithParty(Party.Employer)
                .CreateCohortWithOtherParty();

            _fixture.VerifyCohortCreationWithOtherParty();
        }

        [Test]
        public async Task CreateCohortWithOtherParty_Creates_CohortWithoutAMessage()
        {
            await _fixture
                .WithParty(Party.Employer)
                .WithNoMessage()
                .CreateCohortWithOtherParty();

            _fixture.VerifyCohortCreationWithOtherParty();
        }
        
        [Test]
        public async Task CreateCohort_ThrowsBadRequest_WhenAccountLegalEntityNotFound()
        {
            await _fixture
                .CreateCohort(_fixture.AccountId, 2323);

            _fixture.VerifyException<BadRequestException>();
        }

        [Test]
        public async Task CreateCohort_ThrowsBadRequest_WhenAccountIdDoesNotMatchAccountIdOnLegalEntity()
        {
            await _fixture
                .CreateCohort(4545, _fixture.AccountLegalEntityId);

            _fixture.VerifyException<BadRequestException>();
        }


        [TestCase(Party.Employer, false)]
        [TestCase(Party.Provider, true)]
        [TestCase(Party.TransferSender, true)]
        public async Task CreateCohortWithOtherParty_Throws_If_Not_Employer(Party creatingParty, bool expectThrows)
        {
            await _fixture
                .WithParty(creatingParty)
                .CreateCohortWithOtherParty();

            if (expectThrows)
            {
                _fixture.VerifyException<InvalidOperationException>();
            }
            else
            {
                _fixture.VerifyNoException();
            }
        }

        [Test]
        public async Task AddDraftApprenticeship_Provider_Adds_Draft_Apprenticeship()
        {
            _fixture.WithParty(Party.Employer).WithExistingCohort(Party.Employer);
            await _fixture.AddDraftApprenticeship();
            _fixture.VerifyProviderDraftApprenticeshipAdded();
        }

        [Test]
        public void AddDraftApprenticeship_CohortNotFound_ShouldThrowException()
        {
            Assert.ThrowsAsync<BadRequestException>(_fixture.AddDraftApprenticeship,
                $"Cohort {_fixture.CohortId} was not found");
        }

        [TestCase("2019-07-31", null, true)]
        [TestCase("2019-07-31", "2020-08-01", false, Description = "One day after cut off")]
        [TestCase("2019-07-31", "2020-07-31", true, Description = "Day of cut off (last valid day)")]
        [TestCase("2019-07-31", "2018-01-01", true, Description = "Day in the past")]
        public async Task StartDate_CheckIsWithinAYearOfEndOfCurrentTeachingYear_Validation(
            DateTime academicYearEndDate, DateTime? startDate, bool passes)
        {
            await _fixture
                .WithParty(Party.Provider)
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
                .WithParty(Party.Provider)
                .WithUlnValidationResult(validationResult)
                .CreateCohort();

            _fixture.VerifyUlnException(passes);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public async Task Reservation_Validation(bool hasValidationError, bool passes)
        {
            await _fixture
                .WithParty(Party.Provider)
                .WithReservationValidationResult(hasValidationError)
                .CreateCohort();

            _fixture.VerifyReservationException(passes);
        }

        [Test]
        public async Task Reservation_Validation_Skipped()
        {
            await _fixture.WithParty(Party.Provider).CreateCohort();
            _fixture.VerifyReservationValidationNotPerformed();
        }

        [Test]
        public async Task OverlapOnStartDate_Validation()
        {
            await _fixture.WithParty(Party.Provider).WithUlnOverlapOnStartDate().CreateCohort();
            _fixture.VerifyOverlapExceptionOnStartDate();
        }

        [Test]
        public async Task OverlapOnEndDate_Validation()
        {
            await _fixture.WithParty(Party.Provider).WithUlnOverlapOnEndDate().CreateCohort();
            _fixture.VerifyOverlapExceptionOnEndDate();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task UpdateDraftApprenticeship_IsSuccessful_ThenDraftApprenticeshipIsUpdated(Party withParty)
        {
            _fixture.WithParty(withParty).WithExistingCohort(withParty).WithExistingDraftApprenticeship();
            await _fixture.UpdateDraftApprenticeship();
            _fixture.VerifyDraftApprenticeshipUpdated();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task UpdateDraftApprenticeship_WhenUserInfoDoesExist_ThenLastUpdatedFieldsAreSet(Party withParty)
        {
            _fixture.WithParty(withParty).WithExistingCohort(withParty).WithExistingDraftApprenticeship();
            await _fixture.UpdateDraftApprenticeship();
            _fixture.VerifyLastUpdatedFieldsAreSet(withParty);
        }

        [Test]
        public async Task UpdateDraftApprenticeship_WhenUserInfoDoesNotExist_ThenLastUpdatedFieldsAreNotSet()
        {
            _fixture.WithParty(Party.Employer).WithExistingCohort(Party.Employer).WithExistingDraftApprenticeship().WithNoUserInfo();
            await _fixture.UpdateDraftApprenticeship();
            _fixture.VerifyLastUpdatedFieldsAreNotSet();
        }
        
        [Test]
        public void AddDraftApprenticeship_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties();
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AddDraftApprenticeship());
        }
        
        [Test]
        public void UpdateDraftApprenticeship_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties();
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.UpdateDraftApprenticeship());
        }
        
        [Test]
        public void SendCohortToOtherParty_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties();
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.SendCohortToOtherParty());
        }
        
        [Test]
        public void ApproveCohort_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties();
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.ApproveCohort());
        }

        public class CohortDomainServiceTestFixture
        {
            public DateTime Now { get; set; }
            public CohortDomainService CohortDomainService { get; set; }
            public ProviderCommitmentsDbContext Db { get; set; }
            public long ProviderId { get; }
            public long AccountId { get; }
            public long AccountLegalEntityId { get; }
            public long CohortId { get; }
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; }
            public DraftApprenticeship ExistingDraftApprenticeship { get; }
            public long DraftApprenticeshipId { get; }

            public Mock<Provider> Provider { get; set; }
            public Mock<AccountLegalEntity> AccountLegalEntity { get; set; }
            public Cohort Cohort { get; set; }
            public Mock<IAcademicYearDateProvider> AcademicYearDateProvider { get; }
            public Mock<IUlnValidator> UlnValidator { get; }
            public Mock<IReservationValidationService> ReservationValidationService { get; }
            private Mock<IOverlapCheckService> OverlapCheckService { get; }
            public Party Party { get; set; }
            public Mock<IAuthenticationService> AuthenticationService { get; }
            public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
            public Exception Exception { get; private set; }
            public List<DomainError> DomainErrors { get; }
            public string Message { get; private set; }
            public UserInfo UserInfo { get; private set; }
            public Mock<IChangeTrackingSessionFactory> ChangeTrackingSessionFactory { get; set; }

            public CohortDomainServiceTestFixture()
            {
                Now = DateTime.UtcNow;
                var fixture = new Fixture();

                // We need this to allow the UoW to initialise it's internal static events collection.
                var uow = new UnitOfWorkContext();

                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);

                ProviderId = 1;
                AccountId = 2;
                AccountLegalEntityId = 3;
                CohortId = 4;
                Message = fixture.Create<string>();

                Provider = new Mock<Provider>();
                Provider.Setup(x => x.UkPrn).Returns(ProviderId);
                Db.Providers.Add(Provider.Object);

                AccountLegalEntity = new Mock<AccountLegalEntity>();
                AccountLegalEntity.Setup(x => x.Id).Returns(AccountLegalEntityId);
                AccountLegalEntity.Setup(x => x.AccountId).Returns(AccountId);
                Db.AccountLegalEntities.Add(AccountLegalEntity.Object);

                DraftApprenticeshipId = fixture.Create<long>();

                DraftApprenticeshipDetails = new DraftApprenticeshipDetails
                {
                    FirstName = "Test", LastName = "Test"
                };

                ExistingDraftApprenticeship = new DraftApprenticeship { Id = DraftApprenticeshipId, CommitmentId = CohortId};

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

                AuthenticationService = new Mock<IAuthenticationService>();
                
                CurrentDateTime = new Mock<ICurrentDateTime>();
                CurrentDateTime.Setup(d => d.UtcNow).Returns(Now);

                Exception = null;
                DomainErrors = new List<DomainError>();
                UserInfo = fixture.Create<UserInfo>();

                ChangeTrackingSessionFactory = new Mock<IChangeTrackingSessionFactory>();
                ChangeTrackingSessionFactory
                    .Setup(x => x.CreateTrackingSession(It.IsAny<UserAction>(),
                        It.IsAny<Party>(),
                        It.IsAny<long>(),
                        It.IsAny<long>(),
                        It.IsAny<UserInfo>()))
                    .Returns(Mock.Of<IChangeTrackingSession>());

                CohortDomainService = new CohortDomainService(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                    Mock.Of<ILogger<CohortDomainService>>(),
                    AcademicYearDateProvider.Object,
                    UlnValidator.Object,
                    ReservationValidationService.Object,
                    OverlapCheckService.Object,
                    AuthenticationService.Object,
                    CurrentDateTime.Object,
                    ChangeTrackingSessionFactory.Object);
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

            public CohortDomainServiceTestFixture WithNoMessage()
            {
                Message = null;
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

            public CohortDomainServiceTestFixture WithExistingCohort(Party creatingParty)
            {
                Cohort = new Cohort
                {
                    Id = CohortId,
                    EditStatus = creatingParty.ToEditStatus(),
                    ProviderId = ProviderId
                };
                
                Db.Cohorts.Add(Cohort);

                return this;
            }

            public CohortDomainServiceTestFixture WithExistingCohortApprovedByAllParties()
            {
                Cohort = new Cohort
                {
                    Id = CohortId,
                    EditStatus = EditStatus.Both,
                    TransferSenderId = null
                };
                
                Db.Cohorts.Add(Cohort);

                return this;
            }

            public CohortDomainServiceTestFixture WithExistingDraftApprenticeship()
            {
                DraftApprenticeshipDetails.Id = DraftApprenticeshipId;
                Db.DraftApprenticeships.Add(ExistingDraftApprenticeship);
                return this;
            }

            public CohortDomainServiceTestFixture WithParty(Party party)
            {
                Party = party;
                AuthenticationService.Setup(x => x.GetUserParty()).Returns(Party);
                return this;
            }

            public CohortDomainServiceTestFixture WithNoUserInfo()
            {
                UserInfo = null;
                return this;
            }

            public async Task<Cohort> CreateCohort(long? accountId = null, long? accountLegalEntityId = null)
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                accountId = accountId ?? AccountId;
                accountLegalEntityId = accountLegalEntityId ?? AccountLegalEntityId; 

                try
                {
                    var result = await CohortDomainService.CreateCohort(ProviderId, accountId.Value, accountLegalEntityId.Value,
                        DraftApprenticeshipDetails, UserInfo, new CancellationToken());
                    await Db.SaveChangesAsync();
                    return result;
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                    return null;
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    return null;
                }
            }

            public async Task<Cohort> CreateCohortWithOtherParty()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    var result = await CohortDomainService.CreateCohortWithOtherParty(ProviderId, AccountId, AccountLegalEntityId, Message, UserInfo, new CancellationToken());
                    await Db.SaveChangesAsync();
                    return result;
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                    return null;
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    return null;
                }
            }

            public async Task AddDraftApprenticeship()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    await CohortDomainService.AddDraftApprenticeship(ProviderId, CohortId, DraftApprenticeshipDetails, UserInfo, new CancellationToken());
                    await Db.SaveChangesAsync();
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                }
            }

            public async Task ApproveCohort()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    await CohortDomainService.ApproveCohort(CohortId, Message, UserInfo, new CancellationToken());
                    await Db.SaveChangesAsync();
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                }
            }

            public async Task SendCohortToOtherParty()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    await CohortDomainService.SendCohortToOtherParty(CohortId, Message, UserInfo, new CancellationToken());
                    await Db.SaveChangesAsync();
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                }
            }

            public async Task UpdateDraftApprenticeship()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    await CohortDomainService.UpdateDraftApprenticeship(CohortId, DraftApprenticeshipDetails, UserInfo, new CancellationToken());
                    await Db.SaveChangesAsync();
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                }
            }

            public void VerifyCohortCreation(Party party)
            {
                if (party == Party.Provider)
                {
                    Provider.Verify(x => x.CreateCohort(Provider.Object, AccountLegalEntity.Object,
                        DraftApprenticeshipDetails, UserInfo));
                }

                if (party == Party.Employer)
                {
                    AccountLegalEntity.Verify(x => x.CreateCohort(Provider.Object, AccountLegalEntity.Object,
                        DraftApprenticeshipDetails, UserInfo));
                }
            }

            public void VerifyCohortCreationWithOtherParty()
            {
                AccountLegalEntity.Verify(x => x.CreateCohortWithOtherParty(Provider.Object, Message, UserInfo));
            }

            public void VerifyProviderDraftApprenticeshipAdded()
            {
                Assert.IsTrue(Cohort.DraftApprenticeships.Any());
            }

            public void VerifyDraftApprenticeshipUpdated()
            {
                var updated = Cohort.DraftApprenticeships.SingleOrDefault(x=>x.Id == DraftApprenticeshipId);

                Assert.IsNotNull(updated, "No draft apprenticeship record found");
                Assert.AreEqual(updated.FirstName, DraftApprenticeshipDetails.FirstName);
                Assert.AreEqual(updated.LastName, DraftApprenticeshipDetails.LastName);
            }

            public void VerifyLastUpdatedFieldsAreSet(Party withParty)
            {
                switch (withParty)
                {
                    case Party.Employer:
                        Assert.AreEqual(Cohort.LastUpdatedByEmployerName, UserInfo.UserDisplayName);
                        Assert.AreEqual(Cohort.LastUpdatedByEmployerEmail, UserInfo.UserEmail);
                        break;
                    case Party.Provider:
                        Assert.AreEqual(Cohort.LastUpdatedByProviderName, UserInfo.UserDisplayName);
                        Assert.AreEqual(Cohort.LastUpdatedByProviderEmail, UserInfo.UserEmail);
                        break;
                    default:
                        Assert.Fail("Party must be provider or Employer");
                        break;
                }
            }

            public void VerifyLastUpdatedFieldsAreNotSet()
            {
                Assert.IsNull(Cohort.LastUpdatedByEmployerName);
                Assert.IsNull(Cohort.LastUpdatedByEmployerEmail);
                Assert.IsNull(Cohort.LastUpdatedByProviderName);
                Assert.IsNull(Cohort.LastUpdatedByProviderEmail);
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

            public void VerifyException<T>()
            {
                Assert.IsNotNull(Exception);
                Assert.IsInstanceOf<T>(Exception);
            }

            public void VerifyNoException()
            {
                Assert.IsNull(Exception);
            }
        }
    }
}