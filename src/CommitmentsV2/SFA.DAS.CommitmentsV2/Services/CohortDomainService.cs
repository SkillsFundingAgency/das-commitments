using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class CohortDomainService : ICohortDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAcademicYearDateProvider _academicYearDateProvider;
        private readonly ILogger<CohortDomainService> _logger;
        private readonly IUlnValidator _ulnValidator;
        private readonly IReservationValidationService _reservationValidationService;
        private readonly IOverlapCheckService _overlapCheckService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IEmployerAgreementService _employerAgreementService;
        private readonly IEncodingService _encodingService;
        private readonly IAccountApiClient _accountApiClient;
        private readonly IApprenticeEmailFeatureService _apprenticeEmailFeatureService;

        public CohortDomainService(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<CohortDomainService> logger,
            IAcademicYearDateProvider academicYearDateProvider,
            IUlnValidator ulnValidator,
            IReservationValidationService reservationValidationService,
            IOverlapCheckService overlapCheckService,
            IAuthenticationService authenticationService,
            ICurrentDateTime currentDateTime,
            IEmployerAgreementService employerAgreementService,
            IEncodingService encodingService,
            IAccountApiClient accountApiClient,
            IApprenticeEmailFeatureService apprenticeEmailFeatureService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _academicYearDateProvider = academicYearDateProvider;
            _ulnValidator = ulnValidator;
            _reservationValidationService = reservationValidationService;
            _overlapCheckService = overlapCheckService;
            _authenticationService = authenticationService;
            _currentDateTime = currentDateTime;
            _employerAgreementService = employerAgreementService;
            _encodingService = encodingService;
            _accountApiClient = accountApiClient;
            _apprenticeEmailFeatureService = apprenticeEmailFeatureService;
        }

        public async Task<DraftApprenticeship> AddDraftApprenticeship(long providerId, long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;
            var cohort = await db.GetCohortAggregate(cohortId, cancellationToken);
            var party = _authenticationService.GetUserParty();

            var draftApprenticeship = cohort.AddDraftApprenticeship(draftApprenticeshipDetails, party, userInfo);

            await ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, cohortId, cancellationToken);

            return draftApprenticeship;
        }

        public async Task ApproveCohort(long cohortId, string message, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var cohort = await _dbContext.Value.GetCohortAggregate(cohortId, cancellationToken);
            var party = _authenticationService.GetUserParty();
            var apprenticeEmailIsRequired = _apprenticeEmailFeatureService.IsEnabled &&
                                            _apprenticeEmailFeatureService.ApprenticeEmailIsRequiredFor(cohort.EmployerAccountId, cohort.ProviderId);
            if (party == Party.Employer)
            {
                await ValidateEmployerHasSignedAgreement(cohort, cancellationToken);
            }

            if (party == Party.Provider)
            {
                await ValidateUlnOverlap(cohort);
            }

            await ValidateNoEmailOverlapsExist(cohort, cancellationToken);
            cohort.Approve(party, message, userInfo, _currentDateTime.UtcNow, apprenticeEmailIsRequired);
        }

        private async Task ValidateUlnOverlap(Cohort cohort)
        {
            foreach (var draftApprenticeship  in cohort.DraftApprenticeships)
            {
                if (!string.IsNullOrEmpty(draftApprenticeship.Uln) && draftApprenticeship.StartDate.HasValue && draftApprenticeship.EndDate.HasValue)
                {
                   var result = await  _overlapCheckService.CheckForOverlaps(draftApprenticeship.Uln, draftApprenticeship.StartDate.Value.To(draftApprenticeship.EndDate.Value), draftApprenticeship.Id, CancellationToken.None);
                    if (result.HasOverlaps)
                    {
                        throw new DomainException(draftApprenticeship.Uln, "The draft apprenticeship has overlap");
                    }
                }
            }
        }

        public async Task<Cohort> CreateCohort(long providerId, long accountId, long accountLegalEntityId, long? transferSenderId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var originatingParty = _authenticationService.GetUserParty();
            var db = _dbContext.Value;
            var provider = await GetProvider(providerId, db, cancellationToken);
            var accountLegalEntity = await GetAccountLegalEntity(accountId, accountLegalEntityId, db, cancellationToken);
            var transferSender = transferSenderId.HasValue ? await GetTransferSender(accountId, transferSenderId.Value, db, cancellationToken) : null;
            var originator = GetCohortOriginator(originatingParty, provider, accountLegalEntity);

            await ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, null, cancellationToken);

            return originator.CreateCohort(providerId, accountLegalEntity, transferSender, draftApprenticeshipDetails, userInfo);
        }

        public async Task<Cohort> CreateCohortWithOtherParty(long providerId, long accountId, long accountLegalEntityId, long? transferSenderId, string message, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var originatingParty = _authenticationService.GetUserParty();

            if (originatingParty != Party.Employer)
            {
                throw new InvalidOperationException($"Only Employers can create Cohorts with other party");
            }

            var db = _dbContext.Value;

            var provider = await GetProvider(providerId, db, cancellationToken);
            var accountLegalEntity = await GetAccountLegalEntity(accountId, accountLegalEntityId, db, cancellationToken);
            var transferSender = transferSenderId.HasValue ? await GetTransferSender(accountId, transferSenderId.Value, db, cancellationToken) : null;
            return accountLegalEntity.CreateCohortWithOtherParty(provider.UkPrn, accountLegalEntity, transferSender, message, userInfo);
        }

        public async Task<Cohort> CreateEmptyCohort(long providerId, long accountId, long accountLegalEntityId, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var originatingParty = _authenticationService.GetUserParty();

            if (originatingParty != Party.Provider)
            {
                throw new InvalidOperationException($"Only Providers can create empty cohort");
            }

            var db = _dbContext.Value;
            var provider = await GetProvider(providerId, db, cancellationToken);
            var accountLegalEntity = await GetAccountLegalEntity(accountId, accountLegalEntityId, db, cancellationToken);

            var originator = GetCohortOriginator(originatingParty, provider, accountLegalEntity);

            return originator.CreateCohort(providerId, accountLegalEntity, userInfo);
        }

        public async Task SendCohortToOtherParty(long cohortId, string message, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var cohort = await _dbContext.Value.GetCohortAggregate(cohortId, cancellationToken);
            var party = _authenticationService.GetUserParty();

            cohort.SendToOtherParty(party, message, userInfo, _currentDateTime.UtcNow);
        }

        public async Task<Cohort> UpdateDraftApprenticeship(long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var cohort = await _dbContext.Value.GetCohortAggregate(cohortId, cancellationToken: cancellationToken);
            
            AssertHasProvider(cohortId, cohort.ProviderId);
            AssertHasApprenticeshipId(cohortId, draftApprenticeshipDetails.Id);

            cohort.UpdateDraftApprenticeship(draftApprenticeshipDetails, _authenticationService.GetUserParty(), userInfo);

            if (cohort.IsLinkedToChangeOfPartyRequest)
            {
                await ValidateStartDateForContinuation(cohort, draftApprenticeshipDetails);
            }

            await ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, cohortId, cancellationToken);

            return cohort;
        }

        public async Task<Cohort> DeleteDraftApprenticeship(long cohortId, long apprenticeshipId, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var cohort = await _dbContext.Value.GetCohortAggregate(cohortId, cancellationToken: cancellationToken);

            AssertHasApprenticeshipId(cohortId, apprenticeshipId);
            cohort.DeleteDraftApprenticeship(apprenticeshipId, _authenticationService.GetUserParty(), userInfo);

            return cohort;
        }

        private ICohortOriginator GetCohortOriginator(Party originatingParty, Provider provider, AccountLegalEntity accountLegalEntity)
        {
            switch (originatingParty)
            {
                case Party.Employer:
                    return accountLegalEntity;
                case Party.Provider:
                    return provider;
                default:
                    throw new ArgumentException($"Unable to get ICohortOriginator from Party of type {originatingParty}");
            }
        }

        private void AssertHasProvider(long cohortId, long? providerId)
        {
            if (providerId == null)
            {
                // We need a provider id to validate the apprenticeship with reservations, so a provider id is mandatory.
                throw new InvalidOperationException($"Cannot update cohort {cohortId} because it is not linked to a provider");
            }
        }

        private static void AssertHasApprenticeshipId(long cohortId, long draftApprenticeshipDetailId)
        {
            if (draftApprenticeshipDetailId < 1)
            {
                throw new InvalidOperationException($"Cannot update cohort {cohortId} because the supplied draft apprenticeship does not have an id");
            }
        }

        private static async Task<AccountLegalEntity> GetAccountLegalEntity(long accountId, long accountLegalEntityId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
        {
            var accountLegalEntity =
                await db.AccountLegalEntities.SingleOrDefaultAsync(x => x.Id == accountLegalEntityId,
                    cancellationToken);
            if (accountLegalEntity == null)
                throw new BadRequestException($"AccountLegalEntity {accountLegalEntityId} was not found");
            if (accountLegalEntity.AccountId != accountId)
                throw new BadRequestException($"AccountLegalEntity {accountLegalEntityId} does not belong to the Account {accountId}");

            return accountLegalEntity;
        }

        private static async Task<Account> GetAccount(long accountId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
        {
            var account = await db.Accounts.SingleOrDefaultAsync(x => x.Id == accountId, cancellationToken);
            if (account == null)
                throw new BadRequestException($"Account {accountId} was not found");

            return account;
        }

        private async Task<Account> GetTransferSender(long employerAccountId, long transferSenderId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
        {
            await ValidateTransferSenderIdIsAFundingConnection(employerAccountId, transferSenderId);
            return await GetAccount(transferSenderId, db, cancellationToken);
        }

        private async Task ValidateTransferSenderIdIsAFundingConnection(long accountId, long transferSenderId)
        {
            var hashedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);
            var fundingConnections = await _accountApiClient.GetTransferConnections(hashedAccountId);
            if (fundingConnections.Any(x => x.FundingEmployerAccountId == transferSenderId))
            {
                return;
            }
            throw new BadRequestException($"TransferSenderId {transferSenderId} is not a FundingEmployer for Account {accountId}");
        }

        private static async Task<Provider> GetProvider(long providerId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
        {
            var provider = await db.Providers.SingleOrDefaultAsync(p => p.UkPrn == providerId, cancellationToken);
            if (provider == null) throw new BadRequestException($"Provider {providerId} was not found");
            return provider;
        }

        private async Task ValidateStartDateForContinuation(Cohort cohort, DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (!draftApprenticeshipDetails.StartDate.HasValue) return;

            var existingDraftApprenticeship = cohort.GetDraftApprenticeship(draftApprenticeshipDetails.Id);

            if (!existingDraftApprenticeship.IsContinuation)
            {
                throw new InvalidOperationException(
                    $"Cohort {cohort.Id} is linked to Change Of Party Request {cohort.ChangeOfPartyRequestId} but DraftApprenticeship {existingDraftApprenticeship.Id} is not a Continuation");
            }

            var previousApprenticeship = await
                _dbContext.Value.GetApprenticeshipAggregate(existingDraftApprenticeship.ContinuationOfId.Value, default);

            if (draftApprenticeshipDetails.StartDate.Value < previousApprenticeship.StopDate)
            {
                throw new DomainException(nameof(draftApprenticeshipDetails.StartDate), "The date overlaps with existing dates for the same apprentice.");
            }
        }

        private async Task ValidateDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails, long? cohortId, CancellationToken cancellationToken)
        {
            ValidateStartDate(draftApprenticeshipDetails);
            ValidateUln(draftApprenticeshipDetails);
            await ValidateOverlaps(draftApprenticeshipDetails, cancellationToken);
            await ValidateEmailOverlaps(draftApprenticeshipDetails, cohortId, cancellationToken);
            await ValidateReservation(draftApprenticeshipDetails, cancellationToken);
        }

        private void ValidateUln(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.Uln)) return;

            switch (_ulnValidator.Validate(draftApprenticeshipDetails.Uln))
            {
                case UlnValidationResult.IsInValidTenDigitUlnNumber:
                    throw new DomainException(nameof(draftApprenticeshipDetails.Uln), "You must enter a 10-digit unique learner number");
                case UlnValidationResult.IsInvalidUln:
                    throw new DomainException(nameof(draftApprenticeshipDetails.Uln), "You must enter a valid unique learner number");
            }
        }

        private void ValidateStartDate(DraftApprenticeshipDetails details)
        {
            if (!details.StartDate.HasValue) return;

            if (details.StartDate.Value > _academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
            {
                throw new DomainException(nameof(details.StartDate),
                    "The start date must be no later than one year after the end of the current teaching year");
            }
        }

        private async Task ValidateReservation(DraftApprenticeshipDetails details, CancellationToken cancellationToken)
        {
            if (!details.ReservationId.HasValue || !details.StartDate.HasValue || details.TrainingProgramme == null)
                return;

            var validationRequest = new ReservationValidationRequest(details.ReservationId.Value, details.StartDate.Value, details.TrainingProgramme.CourseCode);

            var validationResult = await _reservationValidationService.Validate(validationRequest, cancellationToken);

            var errors = validationResult.ValidationErrors.Select(error => new DomainError(error.PropertyName, error.Reason)).ToList();
            errors.ThrowIfAny();
        }

        private async Task ValidateOverlaps(DraftApprenticeshipDetails details, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(details.Uln) || !details.StartDate.HasValue || !details.EndDate.HasValue) return;

            var overlapResult = await _overlapCheckService.CheckForOverlaps(details.Uln, details.StartDate.Value.To(details.EndDate.Value), default, cancellationToken);

            if (!overlapResult.HasOverlaps) return;

            var errorMessage = "The date overlaps with existing dates for the same apprentice."
                               + Environment.NewLine +
                               "Please check the date - contact the " + (_authenticationService.GetUserParty() == Party.Employer ? "provider" : "employer") + " for help";

            var errors = new List<DomainError>();

            if (overlapResult.HasOverlappingStartDate)
            {
                errors.Add(new DomainError(nameof(details.StartDate), errorMessage));
            }

            if (overlapResult.HasOverlappingEndDate)
            {
                errors.Add(new DomainError(nameof(details.EndDate), errorMessage));
            }

            throw new DomainException(errors);
        }

        private async Task ValidateEmailOverlaps(DraftApprenticeshipDetails details, long? cohortId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(details.Email) || !details.StartDate.HasValue || !details.EndDate.HasValue) return;

            var overlapCheck = await _overlapCheckService.CheckForEmailOverlaps(details.Email, details.StartDate.Value.To(details.EndDate.Value), details.Id, cohortId, cancellationToken);

            if (overlapCheck == null) return;

            var errorMessage = overlapCheck.BuildErrorMessage();

            var errors = new List<DomainError>();
            errors.Add(new DomainError(nameof(details.Email), errorMessage));

            throw new DomainException(errors);
        }

        private async Task ValidateEmployerHasSignedAgreement(Cohort cohort, CancellationToken cancellationToken)
        {
            async Task<long> GetMaLegalEntityId()
            {
                var accountLegalEntityId = cohort.AccountLegalEntityId;
                var accountLegalEntity = await _dbContext.Value.AccountLegalEntities.Where(x => x.Id == accountLegalEntityId).SingleAsync(cancellationToken);
                return accountLegalEntity.MaLegalEntityId;
            }

            AgreementFeature[] agreementFeatures = new AgreementFeature[0];

            if (cohort.TransferSenderId != null)
            {
                agreementFeatures = new AgreementFeature[] { AgreementFeature.Transfers };
            }
            var isSigned = await _employerAgreementService.IsAgreementSigned(cohort.EmployerAccountId, await GetMaLegalEntityId(), agreementFeatures);

            if (!isSigned)
            {
                throw new DomainException(nameof(cohort.EmployerAccountId), $"Employer {cohort.EmployerAccountId} cannot approve any cohort because the agreement is not signed");
            }
        }
        private async Task ValidateNoEmailOverlapsExist(Cohort cohort, CancellationToken cancellationToken)
        {
            var emailOverlaps = await _overlapCheckService.CheckForEmailOverlaps(cohort.Id, cancellationToken);

            if (emailOverlaps.Any())
            {
                throw new DomainException(nameof(cohort.Id), $"Cannot approve this cohort because one or more emails are failing the overlap check");
            }
        }
    }
}
