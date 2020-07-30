using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
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
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using TestSupport.EfHelpers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class CohortDomainServiceTests
    {
        private CohortDomainServiceTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortDomainServiceTestFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture.TearDown();
            _fixture = null;
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

        [TestCase(Party.Employer)]
        public async Task CreateCohort_CreatingPartyWithTransferSenderId_Creates_Cohort(Party party)
        {
            await _fixture
                .WithParty(party)
                .CreateCohort(null, null, _fixture.TransferSenderId);
            _fixture.VerifyCohortCreationWithTransferSender(party);
        }

        [Test]
        public async Task CreateCohort_WithAnInvalidTransferSenderId_ThrowsBadRequestException()
        {
            await _fixture
                .WithParty(Party.Employer)
                .CreateCohort(null, null, -1);

            _fixture.VerifyException<BadRequestException>();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task CreateCohort_CreatingPartyWithoutTransferSenderId_Creates_Cohort(Party party)
        {
            await _fixture
                .WithParty(party)
                .CreateCohort(null, null, null);
            _fixture.VerifyCohortCreationWithoutTransferSender(party);
        }

        [Test]
        public async Task CreateCohortWithOtherParty_WithNoTransferSenderId_Creates_Cohort()
        {
            await _fixture
                .WithParty(Party.Employer)
                .CreateCohortWithOtherParty();

            _fixture.VerifyCohortCreationWithOtherParty_WithoutTransferSender();
        }

        [Test]
        public async Task CreateCohortWithOtherParty_WithTransferSenderId_Creates_Cohort()
        {
            await _fixture
                .WithParty(Party.Employer)
                .CreateCohortWithOtherParty(_fixture.TransferSenderId);

            _fixture.VerifyCohortCreationWithOtherParty_WithTransferSender();
        }
        [Test]
        public async Task CreateCohortWithOtherParty_WithAnInvalidTransferSenderId_ThrowsBadRequestException()
        {
            await _fixture
                .WithParty(Party.Employer)
                .CreateCohortWithOtherParty(-1);

            _fixture.VerifyException<BadRequestException>();
        }

        [Test]
        public async Task CreateCohortWithOtherParty_Creates_CohortWithoutAMessage()
        {
            await _fixture
                .WithParty(Party.Employer)
                .WithNoMessage()
                .CreateCohortWithOtherParty();

            _fixture.VerifyCohortCreationWithOtherParty_WithoutTransferSender();
        }

        [Test]
        public async Task CreateCohort_ThrowsBadRequest_WhenAccountLegalEntityNotFound()
        {
            await _fixture
                .CreateCohort(_fixture.AccountId, 2323);

            _fixture.VerifyException<BadRequestException>();
        }

        [Test]
        public async Task CreateCohort_ThrowsBadRequest_WhenTransferSenderNotFound()
        {
            await _fixture
                .CreateCohort(null, null, -1);

            _fixture.VerifyException<BadRequestException>();
        }

        [Test]
        public async Task CreateCohortWithOtherParty_ThrowsBadRequest_WhenTransferSenderNotFound()
        {
            await _fixture
                .WithParty(Party.Employer)
                .CreateCohortWithOtherParty(-1);

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
        public async Task CreateEmptyCohort_Creates_EmptyCohort()
        {
            await _fixture
                  .WithParty(Party.Provider)
                  .CreateEmptyCohort();

            _fixture.VerifyEmptyCohortCreation(Party.Provider);
        }

        [TestCase(Party.Employer, true)]
        [TestCase(Party.Provider, false)]
        [TestCase(Party.TransferSender, true)]
        public async Task CreateEmptyCohort_Throws_If_Not_Provider(Party creatingParty, bool expectThrows)
        {
            await _fixture
                .WithParty(creatingParty)
                .CreateEmptyCohort();

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
            _fixture.WithParty(Party.Provider).WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Provider);
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

        [TestCase(Party.Provider, "employer")]
        [TestCase(Party.Employer, "provider")]
        public async Task OverlapOnStartDate_Validation(Party party, string otherPartyInMessage)
        {
            await _fixture.WithParty(party).WithUlnOverlapOnStartDate().CreateCohort();
            _fixture.VerifyOverlapExceptionOnStartDate(otherPartyInMessage);
        }

        [TestCase(Party.Provider, "employer")]
        [TestCase(Party.Employer, "provider")]
        public async Task OverlapOnEndDate_Validation(Party party, string otherPartyInMessage)
        {
            await _fixture.WithParty(party).WithUlnOverlapOnEndDate().CreateCohort();
            _fixture.VerifyOverlapExceptionOnEndDate(otherPartyInMessage);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task UpdateDraftApprenticeship_IsSuccessful_ThenDraftApprenticeshipIsUpdated(Party withParty)
        {
            _fixture.WithParty(withParty).WithCohortMappedToProviderAndAccountLegalEntity(withParty, withParty).WithExistingDraftApprenticeship();
            await _fixture.UpdateDraftApprenticeship();
            _fixture.VerifyDraftApprenticeshipUpdated();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task UpdateDraftApprenticeship_WhenUserInfoDoesExist_ThenLastUpdatedFieldsAreSet(Party withParty)
        {
            _fixture.WithParty(withParty).WithCohortMappedToProviderAndAccountLegalEntity(withParty, withParty).WithExistingDraftApprenticeship();
            await _fixture.UpdateDraftApprenticeship();
            _fixture.VerifyLastUpdatedFieldsAreSet(withParty);
        }

        [Test]
        public async Task UpdateDraftApprenticeship_WhenUserInfoDoesNotExist_ThenLastUpdatedFieldsAreNotSet()
        {
            _fixture.WithParty(Party.Employer).WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer).WithExistingDraftApprenticeship().WithNoUserInfo();
            await _fixture.UpdateDraftApprenticeship();
            _fixture.VerifyLastUpdatedFieldsAreNotSet();
        }

        [Test]
        public void AddDraftApprenticeship_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties(Party.Employer);
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.AddDraftApprenticeship());
        }
        
        [Test]
        public void UpdateDraftApprenticeship_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties(Party.Employer);
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.UpdateDraftApprenticeship());
        }

        [Test]
        public void SendCohortToOtherParty_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties(Party.Employer);
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.SendCohortToOtherParty());
        }

        [Test]
        public void ApproveCohort_WhenCohortIsApprovedByAllParties_ShouldThrowException()
        {
            _fixture.WithExistingCohortApprovedByAllParties(Party.Employer);
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.ApproveCohort());
        }

        [Test]
        public void ApproveCohort_WhenEmployerApprovesAndAgreementIsNotSigned_ShouldThrowException()
        {
            _fixture.WithParty(Party.Employer).WithExistingUnapprovedCohort().WithDecodeOfPublicHashedAccountLegalEntity().WithAgreementSignedAs(false);
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.ApproveCohort());
        }

        [Test]
        [Ignore("Test is faulty. No setup is provided for getting ALE as required. Previous test passed due to expecting InvalidOperationExeption, the condition for which was met but not by the right error in the right place.")]
        public void ApproveCohort_WhenEmployerApprovesAndThereIsATransferSenderAndAgreementIsNotSigned_ShouldThrowException()
        {
            _fixture.WithParty(Party.Employer).WithExistingUnapprovedTransferCohort();
            Assert.ThrowsAsync<DomainException>(() => _fixture.ApproveCohort());
        }

        [Test]
        public async Task ApproveCohort_WhenEmployerApprovesAndAgreementIsSigned_ShouldSucceed()
        {
            _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer)
                .WithDecodeOfPublicHashedAccountLegalEntity()
                .WithAgreementSignedAs(true)
                .WithExistingDraftApprenticeship();

            await _fixture.WithParty(Party.Employer).ApproveCohort();
            _fixture.VerifyIsAgreementSignedIsCalledCorrectly();
        }

        [Test]
        public async Task DeleteDraftApprenticeship_WhenCohortIsWithEmployer()
        {
            _fixture.WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer).WithExistingDraftApprenticeship();
            await _fixture.WithParty(Party.Employer).DeleteDraftApprenticeship();
            _fixture.VerifyDraftApprenticeshipDeleted();
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task UpdateDraftApprenticeship_WhenContinuation_StartDateMustBeAfterPreviousStopDate(bool overlap, bool expectThrow)
        {
            _fixture.WithParty(Party.Employer)
                .WithCohortMappedToProviderAndAccountLegalEntity(Party.Employer, Party.Employer)
                .WithExistingDraftApprenticeship()
                .WithContinuation(overlap);
            await _fixture.UpdateDraftApprenticeship();

            if(expectThrow)
            {
                _fixture.VerifyException<DomainException>();
            }
            else
            {
                _fixture.VerifyNoException();
            }
        }

        [TestCase("2018-04-30", false)]
        [TestCase("2018-05-01", true)]
        public async Task AddDraftApprenticeship_Verify_StartDate_ForTransferSender_Is_After_May_2018(DateTime startDate, bool pass)
        {
            _fixture.WithParty(Party.Employer).WithExistingUnapprovedTransferCohort()
                .WithStartDate(startDate)
                .WithTrainingProgramme();

            await _fixture.AddDraftApprenticeship();

            _fixture.VerifyStartDateException(pass);
        }

        public class CohortDomainServiceTestFixture
        {
            public DateTime Now { get; set; }
            public CohortDomainService CohortDomainService { get; set; }
            public ProviderCommitmentsDbContext Db { get; set; }
            public long ProviderId { get; }
            public long AccountId { get; }
            public long TransferSenderId { get; }
            public string TransferSenderName { get; }
            public long AccountLegalEntityId { get; }
            public long CohortId { get; }
            public string AccountLegalEntityPublicHashedId { get; }
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; }
            public DraftApprenticeship ExistingDraftApprenticeship { get; }
            public Apprenticeship PreviousApprenticeship { get; }
            public long DraftApprenticeshipId { get; }

            public Account EmployerAccount { get; set; }
            public Account TransferSenderAccount { get; set; }
            public Mock<Provider> Provider { get; set; }
            public Mock<AccountLegalEntity> AccountLegalEntity { get; set; }
            public Cohort Cohort { get; set; }
            public Cohort NewCohort { get; set; }
            public Mock<IAcademicYearDateProvider> AcademicYearDateProvider { get; }
            public Mock<IUlnValidator> UlnValidator { get; }
            public Mock<IReservationValidationService> ReservationValidationService { get; }
            public Mock<IEmployerAgreementService> EmployerAgreementService { get; }
            public Mock<IEncodingService> EncodingService { get; }
            private Mock<IOverlapCheckService> OverlapCheckService { get; }
            public Party Party { get; set; }
            public Mock<IAuthenticationService> AuthenticationService { get; }
            public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
            public Mock<IAccountApiClient> AccountApiClient { get; set; }
            public List<TransferConnectionViewModel> TransferConnections { get; }

            public Exception Exception { get; private set; }
            public List<DomainError> DomainErrors { get; }
            public string Message { get; private set; }
            public UserInfo UserInfo { get; private set; }

            public long MaLegalEntityId { get; private set; }

            public CohortDomainServiceTestFixture()
            {
                Now = DateTime.UtcNow;
                var fixture = new Fixture();

                // We need this to allow the UoW to initialise it's internal static events collection.
                var uow = new UnitOfWorkContext();

                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), new InMemoryDatabaseRoot())
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .EnableSensitiveDataLogging()
                    .Options);

                ProviderId = 1;
                AccountId = 2;
                AccountLegalEntityId = 3;
                CohortId = 4;
                MaLegalEntityId = fixture.Create<long>();
                AccountLegalEntityPublicHashedId = fixture.Create<string>();

                Message = fixture.Create<string>();

                NewCohort = new Cohort {Apprenticeships = new List<ApprenticeshipBase> {new DraftApprenticeship()}};

                Provider = new Mock<Provider>(()=> new Provider(ProviderId, "Test Provider", DateTime.UtcNow, DateTime.UtcNow));
                Provider.Setup(x => x.CreateCohort(It.IsAny<long>(), It.IsAny<AccountLegalEntity>(), It.IsAny<UserInfo>()))
                    .Returns(NewCohort);
                Db.Providers.Add(Provider.Object);

                EmployerAccount = new Account(AccountId, "AAAA", "BBBB", "Account 1", DateTime.UtcNow);
                Db.Accounts.Add(EmployerAccount);
                AccountLegalEntity = new Mock<AccountLegalEntity>(()=>
                    new AccountLegalEntity(EmployerAccount,AccountLegalEntityId,MaLegalEntityId,"test","ABC","Test",OrganisationType.CompaniesHouse,"test",DateTime.UtcNow));
                AccountLegalEntity.Setup(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), null,
                        It.IsAny<DraftApprenticeshipDetails>(), It.IsAny<UserInfo>()))
                    .Returns(NewCohort);
                AccountLegalEntity.Setup(x => x.CreateCohortWithOtherParty(ProviderId, It.IsAny<AccountLegalEntity>(), null, 
                        It.IsAny<string>(), It.IsAny<UserInfo>()))
                    .Returns(NewCohort);

                AccountLegalEntity.Setup(x => x.Account).Returns(EmployerAccount);
                AccountLegalEntity.Setup(x => x.Cohorts).Returns(new List<Cohort>());

                Db.AccountLegalEntities.Add(AccountLegalEntity.Object);

                TransferSenderId = 23;
                TransferSenderName = fixture.Create<string>();
                TransferSenderAccount = new Account(TransferSenderId, "XXXX", "ZZZZ", TransferSenderName, new DateTime());
                Db.Accounts.Add(TransferSenderAccount);

                TransferConnections = new List<TransferConnectionViewModel>
                    {new TransferConnectionViewModel {FundingEmployerAccountId = TransferSenderId}};
                

                DraftApprenticeshipId = fixture.Create<long>();

                DraftApprenticeshipDetails = new DraftApprenticeshipDetails
                {
                    FirstName = "Test", LastName = "Test"
                };

                ExistingDraftApprenticeship = new DraftApprenticeship {
                        Id = DraftApprenticeshipId,
                        CommitmentId = CohortId,
                        FirstName = fixture.Create<string>(),
                        LastName = fixture.Create<string>(),
                        Uln = "4860364820",
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(1),
                        CourseCode = fixture.Create<string>(),
                        Cost = fixture.Create<int>()
                };
                ExistingDraftApprenticeship.SetValue(x => x.DateOfBirth, ExistingDraftApprenticeship.StartDate.Value.AddYears(-16));

                PreviousApprenticeship = new Apprenticeship();
                PreviousApprenticeship.SetValue(x => x.Id, fixture.Create<long>());
                PreviousApprenticeship.SetValue(x => x.Cohort, new Cohort());
                Db.Apprenticeships.Add(PreviousApprenticeship);

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

                EmployerAgreementService = new Mock<IEmployerAgreementService>();
                EncodingService = new Mock<IEncodingService>();

                AuthenticationService = new Mock<IAuthenticationService>();
                
                CurrentDateTime = new Mock<ICurrentDateTime>();
                CurrentDateTime.Setup(d => d.UtcNow).Returns(Now);

                AccountApiClient = new Mock<IAccountApiClient>();
                AccountApiClient.Setup(x => x.GetTransferConnections(It.IsAny<string>()))
                    .ReturnsAsync(TransferConnections);

                Exception = null;
                DomainErrors = new List<DomainError>();
                UserInfo = fixture.Create<UserInfo>();

                CohortDomainService = new CohortDomainService(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                    Mock.Of<ILogger<CohortDomainService>>(),
                    AcademicYearDateProvider.Object,
                    UlnValidator.Object,
                    ReservationValidationService.Object,
                    OverlapCheckService.Object,
                    AuthenticationService.Object,
                    CurrentDateTime.Object,
                    EmployerAgreementService.Object,
                    EncodingService.Object,
                    AccountApiClient.Object);

                Db.SaveChanges();
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

            public CohortDomainServiceTestFixture WithTrainingProgramme()
            {
                DraftApprenticeshipDetails.TrainingProgramme = new TrainingProgramme("TEST",
                  "TEST",
                  ProgrammeType.Standard,
                  new DateTime(2016, 1, 1),
                  null);
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

            public CohortDomainServiceTestFixture WithCohortMappedToProviderAndAccountLegalEntity(Party creatingParty, Party withParty = Party.None)
            {
                Cohort = new Cohort
                {
                    Id = CohortId,
					WithParty =  withParty,
                    Originator = creatingParty.ToOriginator(),
                    EditStatus = (withParty == Party.Employer || withParty == Party.Provider) ? withParty.ToEditStatus() : EditStatus.Both,
                    Provider = Provider.Object,
                    ProviderId = ProviderId,
                    EmployerAccountId = AccountId,
                    AccountLegalEntityId = AccountLegalEntityId,
                    AccountLegalEntity = AccountLegalEntity.Object,
                    TransferSenderId = null,
                };


                var cohorts = new List<Cohort> {Cohort};
                
                Provider.Setup(x => x.Cohorts).Returns(cohorts);
                AccountLegalEntity.Setup(x => x.Cohorts).Returns(cohorts);

                Db.Cohorts.Add(Cohort);

                return this;
            }

            public CohortDomainServiceTestFixture WithExistingCohortApprovedByAllParties(Party creatingParty)
            {
                WithCohortMappedToProviderAndAccountLegalEntity(creatingParty, Party.None);
                return this;
            }

            public CohortDomainServiceTestFixture WithExistingUnapprovedCohort()
            {
                Cohort = new Cohort
                {
                    Id = CohortId,
                    EditStatus = EditStatus.Neither,
                    TransferSenderId = null
                };

                Db.Cohorts.Add(Cohort);

                return this;
            }

            public CohortDomainServiceTestFixture WithExistingUnapprovedTransferCohort()
            {
                Cohort = new Cohort
                {
                    Id = CohortId,
                    EditStatus = EditStatus.EmployerOnly,
                    TransferSenderId = 11212,
                    Approvals = Party.None,
                    WithParty = Party.Employer
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

            public CohortDomainServiceTestFixture WithContinuation(bool overlap)
            {
                long? changeOfPartyRequestId = 234;
                ExistingDraftApprenticeship.SetValue(x => x.ContinuationOfId, PreviousApprenticeship.Id);
                Cohort.SetValue(x => x.ChangeOfPartyRequestId, changeOfPartyRequestId);
                DraftApprenticeshipDetails.FirstName = ExistingDraftApprenticeship.FirstName;
                DraftApprenticeshipDetails.LastName = ExistingDraftApprenticeship.LastName;
                DraftApprenticeshipDetails.DateOfBirth = ExistingDraftApprenticeship.DateOfBirth;
                DraftApprenticeshipDetails.Uln = ExistingDraftApprenticeship.Uln;
                DraftApprenticeshipDetails.StartDate = ExistingDraftApprenticeship.StartDate;
                DraftApprenticeshipDetails.TrainingProgramme = new TrainingProgramme(ExistingDraftApprenticeship.CourseCode, "", ProgrammeType.Framework, Now,Now);

                if (overlap)
                {
                    PreviousApprenticeship.SetValue(x => x.StopDate, ExistingDraftApprenticeship.StartDate.Value.AddMonths(1));
                }
                else
                {
                    PreviousApprenticeship.SetValue(x => x.StopDate, ExistingDraftApprenticeship.StartDate.Value.AddMonths(-1));
                }

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

            public CohortDomainServiceTestFixture WithDecodeOfPublicHashedAccountLegalEntity()
            {
                EncodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.PublicAccountLegalEntityId))
                    .Returns(AccountLegalEntityId);
                return this;
            }

            public CohortDomainServiceTestFixture WithAgreementSignedAs(bool signed)
            {
                EmployerAgreementService.Setup(x => x.IsAgreementSigned(It.IsAny<long>(), It.IsAny<long>(), 
                    It.IsAny<AgreementFeature[]>())).ReturnsAsync(signed);
                return this;
            }

            public async Task<Cohort> CreateCohort(long? accountId = null, long? accountLegalEntityId = null, long? transferSenderId = null)
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                accountId = accountId ?? AccountId;
                accountLegalEntityId = accountLegalEntityId ?? AccountLegalEntityId; 

                try
                {
                    var result = await CohortDomainService.CreateCohort(ProviderId, accountId.Value, accountLegalEntityId.Value, transferSenderId,
                        DraftApprenticeshipDetails, UserInfo, new CancellationToken());
                    await Db.SaveChangesAsync();
                    return result;
                }
                catch (DomainException ex)
                {
                    DomainErrors.AddRange(ex.DomainErrors);
                    if(Db.Cohorts.Contains(Cohort)) {Db.Cohorts.Remove(Cohort);}
                    return null;
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    if (Db.Cohorts.Contains(Cohort)) { Db.Cohorts.Remove(Cohort); }
                    return null;
                }
            }

            public async Task<Cohort> CreateCohortWithOtherParty(long? transferSenderId = null)
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    var result = await CohortDomainService.CreateCohortWithOtherParty(ProviderId, AccountId, AccountLegalEntityId, transferSenderId, Message, UserInfo, new CancellationToken());
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

            public async Task<Cohort> CreateEmptyCohort()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    var result = await CohortDomainService.CreateEmptyCohort(ProviderId, AccountId, AccountLegalEntityId, UserInfo, new CancellationToken());
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
                if (Party == Party.Employer)
                {
                    DraftApprenticeshipDetails.Uln = ExistingDraftApprenticeship.Uln;
                }

                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    await CohortDomainService.UpdateDraftApprenticeship(CohortId, DraftApprenticeshipDetails, UserInfo, new CancellationToken());
                    await Db.SaveChangesAsync();
                }
                catch (DomainException ex)
                {
                    Exception = ex;
                    DomainErrors.AddRange(ex.DomainErrors);
                }
            }

            public async Task DeleteDraftApprenticeship()
            {
                Db.SaveChanges();
                DomainErrors.Clear();

                try
                {
                    await CohortDomainService.DeleteDraftApprenticeship(CohortId, DraftApprenticeshipId, UserInfo, new CancellationToken());
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
                    Provider.Verify(x => x.CreateCohort(ProviderId, It.Is<AccountLegalEntity>(p=>p == AccountLegalEntity.Object), null,
                        DraftApprenticeshipDetails, UserInfo));
                }

                if (party == Party.Employer)
                {
                    AccountLegalEntity.Verify(x => x.CreateCohort(ProviderId, It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object), null,
                        DraftApprenticeshipDetails, UserInfo));
                }
            }

            public void VerifyEmptyCohortCreation(Party party)
            {
                if (party == Party.Provider)
                {
                    Provider.Verify(x => x.CreateCohort(ProviderId, It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object), UserInfo));
                }
            }

            public void VerifyCohortCreationWithTransferSender(Party party)
            {
                if (party == Party.Provider)
                {
                    Provider.Verify(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), UserInfo));
                }

                if (party == Party.Employer)
                {
                    AccountLegalEntity.Verify(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), It.Is<Account>(t => t.Id == TransferSenderId && t.Name == TransferSenderName),
                        DraftApprenticeshipDetails, UserInfo));
                }
            }

            public void VerifyCohortCreationWithoutTransferSender(Party party)
            {
                if (party == Party.Provider)
                {
                    Provider.Verify(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), It.Is<Account>(p => p == null),
                        DraftApprenticeshipDetails, UserInfo));
                }

                if (party == Party.Employer)
                {
                    AccountLegalEntity.Verify(x => x.CreateCohort(ProviderId, It.IsAny<AccountLegalEntity>(), It.Is<Account>(p => p == null),
                        DraftApprenticeshipDetails, UserInfo));
                }
            }

            public void VerifyCohortCreationWithOtherParty_WithoutTransferSender()
            {
                AccountLegalEntity.Verify(x => x.CreateCohortWithOtherParty(ProviderId, It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object), It.Is<Account>(t => t == null), Message, UserInfo));
            }

            public void VerifyCohortCreationWithOtherParty_WithTransferSender()
            {
                AccountLegalEntity.Verify(x => x.CreateCohortWithOtherParty(ProviderId,
                    It.Is<AccountLegalEntity>(p => p == AccountLegalEntity.Object),
                    It.Is<Account>(t => t.Id == TransferSenderId && t.Name == TransferSenderName),
                    Message,
                    UserInfo));
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

            public void VerifyOverlapExceptionOnStartDate(string otherParty)
            {
                Assert.IsTrue(DomainErrors.Any(x => x.PropertyName == "StartDate" && x.ErrorMessage.Contains($"contact the {otherParty}")));
            }

            public void VerifyOverlapExceptionOnEndDate(string otherParty)
            {
                Assert.IsTrue(DomainErrors.Any(x => x.PropertyName == "EndDate" && x.ErrorMessage.Contains($"contact the {otherParty}")));
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

            public void VerifyIsAgreementSignedIsCalledCorrectly()
            {
                EmployerAgreementService.Verify(x => x.IsAgreementSigned(AccountId, MaLegalEntityId, 
                    It.IsAny<AgreementFeature[]>()));
            }

            public void VerifyDraftApprenticeshipDeleted()
            {
                var deleted = Cohort.DraftApprenticeships.SingleOrDefault(x => x.Id == DraftApprenticeshipId);

                Assert.IsNull(deleted, "Draft apprenticeship record not deleted");
            }

            public void TearDown()
            {
                Db.Database.EnsureDeleted();
            }
        }
    }
}