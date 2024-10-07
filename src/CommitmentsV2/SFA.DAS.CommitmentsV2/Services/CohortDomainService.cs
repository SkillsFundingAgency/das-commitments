using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Services;

public class CohortDomainService(
    Lazy<ProviderCommitmentsDbContext> dbContext,
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
    IEmailOptionalService emailOptionalService,
    ILevyTransferMatchingApiClient levyTransferMatchingApiClient)
    : ICohortDomainService
{
    public async Task<DraftApprenticeship> AddDraftApprenticeship(long providerId, long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, Party? requestingParty, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;
        var cohort = await db.GetCohortAggregate(cohortId, cancellationToken);
        var party = requestingParty ?? authenticationService.GetUserParty();
        var draftApprenticeship = cohort.AddDraftApprenticeship(draftApprenticeshipDetails, party, userInfo);
        await ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, cohort.Id, cancellationToken);
        return draftApprenticeship;
    }

    public async Task ValidateDraftApprenticeshipForOverlappingTrainingDateRequest(long providerId, long? cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken)
    {
        Cohort cohort = null;
        draftApprenticeshipDetails.IgnoreStartDateOverlap = true;
        await ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, cohortId, cancellationToken);
        if (cohortId.HasValue && cohortId.Value > 0)
        {
            cohort = await dbContext.Value.GetCohortAggregate(cohortId.Value, cancellationToken: cancellationToken);
        }

        var isContinuation = cohort != null && cohort.ChangeOfPartyRequestId.HasValue;

        var errors = draftApprenticeshipDetails.ValidateDraftApprenticeshipDetails(isContinuation, cohort?.TransferSenderId, cohort?.Apprenticeships);
        errors.ThrowIfAny();
    }

    public async Task<IEnumerable<Cohort>> AddDraftApprenticeships(List<DraftApprenticeshipDetails> draftApprenticeships, List<BulkUploadAddDraftApprenticeshipRequest> csvBulkUploadApprenticehips, long providerId, UserInfo userInfo, CancellationToken cancellationToken)
    {
        var newCohorts = new Dictionary<long, Cohort>();
        var existingCohorts = new Dictionary<long, Cohort>();

        var db = dbContext.Value;
        var party = authenticationService.GetUserParty();

        foreach (var apprenticeship in draftApprenticeships)
        {
            Cohort cohort;
            logger.LogInformation("Bulk upload - Add draft apprenticeship. Reservation-Id:{ReservationId} - uln {Uln}", apprenticeship.ReservationId, apprenticeship.Uln);
            var csvApprenticeship = csvBulkUploadApprenticehips.First(x => x.Uln == apprenticeship.Uln);

            if (csvApprenticeship.CohortId.HasValue)
            {
                if (existingCohorts.ContainsKey(csvApprenticeship.CohortId.Value))
                {
                    cohort = existingCohorts.GetValueOrDefault(csvApprenticeship.CohortId.Value);
                }
                else
                {
                    cohort = await db.Cohorts.Include(c => c.Apprenticeships)
                        .Include(c => c.AccountLegalEntity)
                        .SingleOrDefaultAsync(c => c.Id == csvApprenticeship.CohortId.Value, cancellationToken);

                    existingCohorts.Add(csvApprenticeship.CohortId.Value, cohort);
                }
            }
            else
            {
                // Check if we have already created an empty cohort for this Legal entity
                if (newCohorts.ContainsKey(csvApprenticeship.LegalEntityId.Value))
                {
                    logger.LogInformation("Bulk upload - Adding to already created cohort - uln {Uln}", apprenticeship.Uln);
                    
                    cohort = newCohorts.GetValueOrDefault(csvApprenticeship.LegalEntityId.Value);
                }
                else
                {
                    logger.LogInformation("Bulk upload - Creating a new cohort for - uln {Uln}", apprenticeship.Uln);
                    
                    var accountLegalEntity = db.AccountLegalEntities
                        .Include(x => x.Account)
                        .First(x => x.Id == csvApprenticeship.LegalEntityId);

                    // create a new cohort for this legal entity
                    cohort = await CreateEmptyCohort(providerId, accountLegalEntity.Account.Id, accountLegalEntity.Id, userInfo, cancellationToken);
                    newCohorts.Add(accountLegalEntity.Id, cohort);
                    db.Cohorts.Add(cohort);
                }
            }

            cohort.AddDraftApprenticeship(apprenticeship, party, userInfo);
            await ValidateDraftApprenticeshipDetails(apprenticeship, null, cancellationToken); // As it is a newly cohort, and not yet saved to db - the cohort Id is null
        }

        return existingCohorts.Select(x => x.Value).Union(newCohorts.Select(x => x.Value));
    }

    public async Task ApproveCohort(long cohortId, string message, UserInfo userInfo, Party? requestingParty, CancellationToken cancellationToken)
    {
        var cohort = await dbContext.Value.GetCohortAggregate(cohortId, cancellationToken);
        var party = requestingParty ?? authenticationService.GetUserParty();
        var apprenticeEmailIsRequired = emailOptionalService.ApprenticeEmailIsRequiredFor(cohort.EmployerAccountId, cohort.ProviderId);

        if (party == Party.Employer)
        {
            await ValidateEmployerHasSignedAgreement(cohort, cancellationToken);
        }

        await ValidateUlnOverlap(cohort);
        await ValidateNoEmailOverlapsExist(cohort, cancellationToken);
        // removed until RPL Reduction warning is elevated to be a error again 
        //await CheckRplReductionErrors(cohort);
        cohort.Approve(party, message, userInfo, currentDateTime.UtcNow, apprenticeEmailIsRequired);
    }
    
    private async Task ValidateUlnOverlap(Cohort cohort)
    {
        foreach (var draftApprenticeship  in cohort.DraftApprenticeships)
        {
            if (string.IsNullOrEmpty(draftApprenticeship.Uln) || !draftApprenticeship.StartDate.HasValue || !draftApprenticeship.EndDate.HasValue)
            {
                continue;
            }

            var result = await  overlapCheckService.CheckForOverlaps(draftApprenticeship.Uln, draftApprenticeship.StartDate.Value.To(draftApprenticeship.EndDate.Value), draftApprenticeship.Id, CancellationToken.None);
            if (result.HasOverlaps)
            {
                throw new DomainException(draftApprenticeship.Uln, "The draft apprenticeship has overlap");
            }
        }
    }

    public async Task<Cohort> CreateCohort(long providerId, long accountId, long accountLegalEntityId, long? transferSenderId, int? pledgeApplicationId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, Party? requestingParty, CancellationToken cancellationToken)
    {
        var originatingParty = requestingParty ?? authenticationService.GetUserParty();
        var db = dbContext.Value;
        var provider = await GetProvider(providerId, db, cancellationToken);
        var accountLegalEntity = await GetAccountLegalEntity(accountId, accountLegalEntityId, db, cancellationToken);
        var transferSender = transferSenderId.HasValue ? await GetTransferSender(accountId, transferSenderId.Value, pledgeApplicationId, db, cancellationToken) : null;
        var originator = GetCohortOriginator(originatingParty, provider, accountLegalEntity);

        await ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, null, cancellationToken);

        return originator.CreateCohort(providerId, accountLegalEntity, transferSender, pledgeApplicationId, draftApprenticeshipDetails, userInfo);
    }

    public async Task<Cohort> CreateCohortWithOtherParty(long providerId, long accountId, long accountLegalEntityId, long? transferSenderId, int? pledgeApplicationId, string message, UserInfo userInfo, CancellationToken cancellationToken)
    {
        var originatingParty = authenticationService.GetUserParty();

        if (originatingParty != Party.Employer)
        {
            throw new InvalidOperationException($"Only Employers can create Cohorts with other party");
        }

        var db = dbContext.Value;

        var provider = await GetProvider(providerId, db, cancellationToken);
        var accountLegalEntity = await GetAccountLegalEntity(accountId, accountLegalEntityId, db, cancellationToken);
        var transferSender = transferSenderId.HasValue ? await GetTransferSender(accountId, transferSenderId.Value, pledgeApplicationId, db, cancellationToken) : null;
        return accountLegalEntity.CreateCohortWithOtherParty(provider.UkPrn, accountLegalEntity, transferSender, pledgeApplicationId, message, userInfo);
    }

    public async Task<Cohort> CreateEmptyCohort(long providerId, long accountId, long accountLegalEntityId, UserInfo userInfo, CancellationToken cancellationToken)
    {
        var originatingParty = authenticationService.GetUserParty();

        if (originatingParty != Party.Provider)
        {
            throw new InvalidOperationException($"Only Providers can create empty cohort");
        }

        var db = dbContext.Value;
        var provider = await GetProvider(providerId, db, cancellationToken);
        var accountLegalEntity = await GetAccountLegalEntity(accountId, accountLegalEntityId, db, cancellationToken);

        var originator = GetCohortOriginator(originatingParty, provider, accountLegalEntity);

        return originator.CreateCohort(providerId, accountLegalEntity, userInfo);
    }

    public async Task SendCohortToOtherParty(long cohortId, string message, UserInfo userInfo, Party? requestingParty, CancellationToken cancellationToken)
    {
        var cohort = await dbContext.Value.GetCohortAggregate(cohortId, cancellationToken);
        var party = requestingParty ?? authenticationService.GetUserParty();

        cohort.SendToOtherParty(party, message, userInfo, currentDateTime.UtcNow);
    }

    public async Task<Cohort> UpdateDraftApprenticeship(long cohortId, DraftApprenticeshipDetails draftApprenticeshipDetails, UserInfo userInfo, Party? requestingParty, CancellationToken cancellationToken)
    {
        var cohort = await dbContext.Value.GetCohortAggregate(cohortId, cancellationToken: cancellationToken);
            
        AssertHasProvider(cohortId, cohort.ProviderId);
        AssertHasApprenticeshipId(cohortId, draftApprenticeshipDetails.Id);

        cohort.UpdateDraftApprenticeship(draftApprenticeshipDetails, requestingParty ?? authenticationService.GetUserParty(), userInfo);

        if (cohort.IsLinkedToChangeOfPartyRequest)
        {
            await ValidateStartDateForContinuation(cohort, draftApprenticeshipDetails, cancellationToken);
        }

        await ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, cohortId, cancellationToken);

        return cohort;
    }

    public async Task<Cohort> DeleteDraftApprenticeship(long cohortId, long apprenticeshipId, UserInfo userInfo, CancellationToken cancellationToken)
    {
        var cohort = await dbContext.Value.GetCohortAggregate(cohortId, cancellationToken: cancellationToken);

        AssertHasApprenticeshipId(cohortId, apprenticeshipId);
        
        cohort.DeleteDraftApprenticeship(apprenticeshipId, authenticationService.GetUserParty(), userInfo);

        return cohort;
    }
    
    private static ICohortOriginator GetCohortOriginator(Party originatingParty, Provider provider, AccountLegalEntity accountLegalEntity)
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

    private static void AssertHasProvider(long cohortId, long? providerId)
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
        {
            throw new BadRequestException($"AccountLegalEntity {accountLegalEntityId} was not found");
        }

        if (accountLegalEntity.AccountId != accountId)
        {
            throw new BadRequestException($"AccountLegalEntity {accountLegalEntityId} does not belong to the Account {accountId}");
        }

        return accountLegalEntity;
    }

    private static async Task<Account> GetAccount(long accountId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if (account == null)
        {
            throw new BadRequestException($"Account {accountId} was not found");
        }

        return account;
    }

    private async Task<Account> GetTransferSender(long employerAccountId, long transferSenderId, int? pledgeApplicationId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
    {
        if (pledgeApplicationId.HasValue)
        {
            await ValidatePledgeApplicationId(employerAccountId, transferSenderId, pledgeApplicationId.Value);
        }
        else
        {
            await ValidateTransferSenderIdIsAFundingConnection(employerAccountId, transferSenderId);
        }
            
        return await GetAccount(transferSenderId, db, cancellationToken);
    }


    private async Task ValidatePledgeApplicationId(long accountId, long transferSenderId, int pledgeApplicationId)
    {
        var pledgeApplication = await levyTransferMatchingApiClient.GetPledgeApplication(pledgeApplicationId);

        if (pledgeApplication == null)
        {
            throw new BadRequestException($"PledgeApplication {pledgeApplicationId} was not found");
        }

        if (pledgeApplication.ReceiverEmployerAccountId != accountId)
        {
            throw new BadRequestException($"PledgeApplication {pledgeApplicationId} does not belong to {accountId}");
        }

        if (pledgeApplication.SenderEmployerAccountId != transferSenderId)
        {
            throw new BadRequestException($"PledgeApplication {pledgeApplicationId} creator {pledgeApplication.SenderEmployerAccountId} does not match supplied Transfer Sender {transferSenderId}");
        }

        if (pledgeApplication.Status != PledgeApplication.ApplicationStatus.Accepted)
        {
            throw new BadRequestException($"PledgeApplication {pledgeApplicationId} has a status of {pledgeApplication.Status}, which is invalid for use");
        }
    }

    private async Task ValidateTransferSenderIdIsAFundingConnection(long accountId, long transferSenderId)
    {
        var hashedAccountId = encodingService.Encode(accountId, EncodingType.AccountId);
        var fundingConnections = await accountApiClient.GetTransferConnections(hashedAccountId);
        if (fundingConnections.Any(x => x.FundingEmployerAccountId == transferSenderId))
        {
            return;
        }
        throw new BadRequestException($"TransferSenderId {transferSenderId} is not a FundingEmployer for Account {accountId}");
    }

    private static async Task<Provider> GetProvider(long providerId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
    {
        var provider = await db.Providers.SingleOrDefaultAsync(p => p.UkPrn == providerId, cancellationToken);
        if (provider == null)
        {
            throw new BadRequestException($"Provider {providerId} was not found");
        }

        return provider;
    }

    private async Task ValidateStartDateForContinuation(Cohort cohort, DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken)
    {         
        if (!draftApprenticeshipDetails.StartDate.HasValue)
        {
            return;
        }

        var existingDraftApprenticeship = cohort.GetDraftApprenticeship(draftApprenticeshipDetails.Id);

        if (!existingDraftApprenticeship.IsContinuation)
        {
            throw new InvalidOperationException(
                $"Cohort {cohort.Id} is linked to Change Of Party Request {cohort.ChangeOfPartyRequestId} but DraftApprenticeship {existingDraftApprenticeship.Id} is not a Continuation");
        }

        var overlappingTrainingDateRequest = await dbContext.Value.OverlappingTrainingDateRequests
            .Where(x => x.DraftApprenticeshipId == draftApprenticeshipDetails.Id)
            .ToListAsync(cancellationToken);

        if (overlappingTrainingDateRequest.Count > 0)
        {
            return;
        }

        var previousApprenticeship = await
            dbContext.Value.GetApprenticeshipAggregate(existingDraftApprenticeship.ContinuationOfId.Value, default);

        if (draftApprenticeshipDetails.StartDate.Value < previousApprenticeship.StopDate)
        {
            throw new DomainException(nameof(draftApprenticeshipDetails.StartDate), "The date overlaps with existing dates for the same apprentice.");
        }
    }

    private async Task ValidateDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails, long? cohortId, CancellationToken cancellationToken)
    {
        ValidateApprenticeshipDate(draftApprenticeshipDetails);
        ValidateUln(draftApprenticeshipDetails);
        await ValidateOverlaps(draftApprenticeshipDetails, cancellationToken);
        await ValidateEmailOverlaps(draftApprenticeshipDetails, cohortId, cancellationToken);
        await ValidateReservation(draftApprenticeshipDetails, cancellationToken);
    }

    private void ValidateUln(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.Uln))
        {
            return;
        }

        switch (ulnValidator.Validate(draftApprenticeshipDetails.Uln))
        {
            case UlnValidationResult.IsInValidTenDigitUlnNumber:
                throw new DomainException(nameof(draftApprenticeshipDetails.Uln), "You must enter a 10-digit unique learner number");
            case UlnValidationResult.IsInvalidUln:
                throw new DomainException(nameof(draftApprenticeshipDetails.Uln), "You must enter a valid unique learner number");
        }
    }

    private void ValidateApprenticeshipDate(DraftApprenticeshipDetails details)
    {
        if (!details.StartDate.HasValue && !details.ActualStartDate.HasValue)
        {
            return;
        }

        var startDate = details.StartDate.HasValue ? details.StartDate.Value : details.ActualStartDate.Value;
        var startDateField = details.StartDate.HasValue ? nameof(details.StartDate) : nameof(details.ActualStartDate);

        if (startDate > academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
        {
            throw new DomainException(startDateField, "The start date must be no later than one year after the end of the current teaching year");
        }

        if (startDate < Domain.Constants.DasStartDate)
        {
            throw new DomainException(startDateField, "The start date must not be earlier than May 2017");
        }

        if (!details.EndDate.HasValue)
        {
            return;
        }

        if (details.EndDate.Value < Domain.Constants.DasStartDate)
        {
            throw new DomainException(nameof(details.EndDate), "The end date must not be earlier than May 2017");
        }
    }

    private async Task ValidateReservation(DraftApprenticeshipDetails details, CancellationToken cancellationToken)
    {
        if (!details.ReservationId.HasValue || !details.StartDate.HasValue || details.TrainingProgramme == null)
        {
            return;
        }

        var validationRequest = new ReservationValidationRequest(details.ReservationId.Value, details.StartDate.Value, details.TrainingProgramme.CourseCode);

        var validationResult = await reservationValidationService.Validate(validationRequest, cancellationToken);

        var errors = validationResult.ValidationErrors.Select(error => new DomainError(error.PropertyName, error.Reason)).ToList();
        errors.ThrowIfAny();
    }
         
    private async Task ValidateOverlaps(DraftApprenticeshipDetails details, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(details.Uln) || !details.StartDate.HasValue || !details.EndDate.HasValue)
        {
            return;
        }

        var overlapResult = await overlapCheckService.CheckForOverlaps(details.Uln, details.StartDate.Value.To(details.EndDate.Value), default, cancellationToken);

        if (!overlapResult.HasOverlaps)
        {
            return;
        }

        var errorMessage = "The date overlaps with existing dates for the same apprentice."
                           + Environment.NewLine +
                           "Please check the date - contact the " + (authenticationService.GetUserParty() == Party.Employer ? "provider" : "employer") + " for help";

        var errors = new List<DomainError>();

        if ((!details.IgnoreStartDateOverlap || overlapResult.HasOverlappingEndDate) && overlapResult.HasOverlappingStartDate)
        {
            errors.Add(new DomainError(nameof(details.StartDate), errorMessage));
        }

        if (overlapResult.HasOverlappingEndDate)
        {
            errors.Add(new DomainError(nameof(details.EndDate), errorMessage));
        }

        if (errors.Count > 0)
        {
            throw new DomainException(errors);
        }
    }

    private async Task ValidateEmailOverlaps(DraftApprenticeshipDetails details, long? cohortId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(details.Email) || !details.StartDate.HasValue || !details.EndDate.HasValue)
        {
            return;
        }

        var overlapCheck = await overlapCheckService.CheckForEmailOverlaps(details.Email, details.StartDate.Value.To(details.EndDate.Value), details.Id, cohortId, cancellationToken);

        if (overlapCheck == null)
        {
            return;
        }

        var errorMessage = overlapCheck.BuildErrorMessage();

        var errors = new List<DomainError>();
        errors.Add(new DomainError(nameof(details.Email), errorMessage));

        throw new DomainException(errors);
    }

    private async Task ValidateEmployerHasSignedAgreement(Cohort cohort, CancellationToken cancellationToken)
    {
        AgreementFeature[] agreementFeatures = [];

        if (cohort.TransferSenderId != null)
        {
            agreementFeatures = [AgreementFeature.Transfers];
        }
        var isSigned = await employerAgreementService.IsAgreementSigned(cohort.EmployerAccountId, await GetMaLegalEntityId(), agreementFeatures);

        if (!isSigned)
        {
            throw new DomainException(nameof(cohort.EmployerAccountId), $"Employer {cohort.EmployerAccountId} cannot approve any cohort because the agreement is not signed");
        }

        return;

        async Task<long> GetMaLegalEntityId()
        {
            var accountLegalEntityId = cohort.AccountLegalEntityId;
            var accountLegalEntity = await dbContext.Value.AccountLegalEntities.Where(x => x.Id == accountLegalEntityId).SingleAsync(cancellationToken);
            return accountLegalEntity.MaLegalEntityId;
        }
    }
    private async Task ValidateNoEmailOverlapsExist(Cohort cohort, CancellationToken cancellationToken)
    {
        var emailOverlaps = await overlapCheckService.CheckForEmailOverlaps(cohort.Id, cancellationToken);

        if (emailOverlaps.Count != 0)
        {
            throw new DomainException(nameof(cohort.Id), $"Cannot approve this cohort because one or more emails are failing the overlap check");
        }
    }
}