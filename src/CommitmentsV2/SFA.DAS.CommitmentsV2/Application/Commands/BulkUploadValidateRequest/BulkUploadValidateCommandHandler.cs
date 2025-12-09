using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.LinkGeneration;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler(
    ILogger<BulkUploadValidateCommandHandler> logger,
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IOverlapCheckService overlapService,
    IAcademicYearDateProvider academicYearDateProvider,
    IProviderRelationshipsApiClient providerRelationshipsApiClient,
    IEmployerAgreementService employerAgreementService,
    RplSettingsConfiguration rplConfig,
    IUlnValidator ulnValidator,
    ILinkGenerator urlHelper)
    : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
{
    private readonly EmployerSummaries _employerSummaries = [];
    private List<BulkUploadAddDraftApprenticeshipRequest> _csvRecords;
    private readonly Dictionary<string, Cohort> _cachedCohortDetails = new();

    public long ProviderId { get; set; }

    public async Task<BulkUploadValidateApiResponse> Handle(BulkUploadValidateCommand command, CancellationToken cancellationToken)
    {
        ProviderId = command.ProviderId;
        var bulkUploadValidationErrors = new List<BulkUploadValidationError>();
        _csvRecords = command.CsvRecords.ToList();

        var standardsError = ValidateHasDeclaredStandards(command.ProviderStandardResults, bulkUploadValidationErrors);

        if (standardsError.Count != 0)
        {
            return new BulkUploadValidateApiResponse
            {
                BulkUploadValidationErrors = standardsError
            };
        }

        foreach (var csvRecord in command.CsvRecords)
        {
            var criticalDomainError = await ValidateCriticalErrors(csvRecord, command.ProviderId);
            await AddError(bulkUploadValidationErrors, csvRecord, criticalDomainError);

            if (criticalDomainError.Count != 0)
            {
                continue;
            }
            
            var domainErrors = await Validate(csvRecord, command.ProviderId, command.ReservationValidationResults, command.ProviderStandardResults, command.OtjTrainingHours);
            
            await AddError(bulkUploadValidationErrors, csvRecord, domainErrors);
        }

        return new BulkUploadValidateApiResponse
        {
            BulkUploadValidationErrors = bulkUploadValidationErrors
        };
    }

    private async Task AddError(List<BulkUploadValidationError> bulkUploadValidationErrors, BulkUploadAddDraftApprenticeshipRequest csvRecord, List<Error> domainErrors)
    {
        if (domainErrors.Count != 0)
        {
            bulkUploadValidationErrors.Add(new BulkUploadValidationError(
                csvRecord.RowNumber,
                await GetEmployerName(csvRecord.AgreementId),
                csvRecord.Uln,
                csvRecord.FirstName + " " + csvRecord.LastName,
                domainErrors
            ));
        }
    }

    private async Task<List<Error>> ValidateCriticalErrors(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId)
    {
        var domainErrors = await ValidateAgreementIdValidFormat(csvRecord);
        if (domainErrors.Count == 0)
        {
            domainErrors.AddRange(await ValidateAgreementIdIsSigned(csvRecord));

            // when a valid agreement has not been signed validation will stop
            if (domainErrors.Count != 0)
            {
                return domainErrors;
            }
        }

        var employerDetails = await GetEmployerDetails(csvRecord.AgreementId);
        if (((employerDetails.IsLevy.HasValue && !employerDetails.IsLevy.Value) || string.IsNullOrEmpty(csvRecord.CohortRef)) 
            && !IsFundedByTransfer(csvRecord.CohortRef) && !await ValidatePermissionToCreateCohort(csvRecord, providerId, domainErrors, employerDetails.IsLevy))
        {
            // when a provider doesn't have permission to create cohort or reserve funding (non-levy) - the validation will stop
            return domainErrors;
        }

        return domainErrors;
    }

    private static List<BulkUploadValidationError> ValidateHasDeclaredStandards(ProviderStandardResults providerStandardResults, List<BulkUploadValidationError> bulkUploadValidationErrors)
    {
        var domainErrors = ValidateDeclaredStandards(providerStandardResults);

        if (domainErrors.Count != 0)
        {
            bulkUploadValidationErrors.Add(new BulkUploadValidationError(
                0,
                null,
                null,
                null,
                domainErrors
            ));
        }

        return bulkUploadValidationErrors;
    }

    /// <summary>
    /// If it is funded by Transfer - non-levy employer doesn't need to check for the permission to create cohort.
    /// </summary>
    /// <param name="cohortRef"></param>
    /// <returns></returns>
    private bool IsFundedByTransfer(string cohortRef)
    {
        if (string.IsNullOrWhiteSpace(cohortRef))
        {
            return false;
        }
        
        var cohortDetails = GetCohortDetails(cohortRef);

        return cohortDetails.TransferSenderId.HasValue;
    }

    private async Task<List<Error>> Validate(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId, BulkReservationValidationResults reservationValidationResults, ProviderStandardResults providerStandardResults, Dictionary<string, int?> otjTrainingHours)
    {
        var domainErrors = await ValidateAgreementIdValidFormat(csvRecord);
        
        if (domainErrors.Count == 0)
        {
            domainErrors.AddRange(await ValidateAgreementIdIsSigned(csvRecord));

            // when a valid agreement has not been signed validation will stop
            if (domainErrors.Count != 0)
            {
                return domainErrors;
            }
        }

        domainErrors.AddRange(await ValidateCohortRef(csvRecord, providerId));
        domainErrors.AddRange(ValidateUln(csvRecord));
        domainErrors.AddRange(ValidateFamilyName(csvRecord));
        domainErrors.AddRange(ValidateGivenName(csvRecord));
        domainErrors.AddRange(ValidateDateOfBirth(csvRecord, providerStandardResults));
        domainErrors.AddRange(ValidateEmailAddress(csvRecord));
        domainErrors.AddRange(ValidateCourseCode(csvRecord, providerStandardResults));
        domainErrors.AddRange(ValidateStartDate(csvRecord));
        domainErrors.AddRange(ValidateEndDate(csvRecord));
        domainErrors.AddRange(ValidateCost(csvRecord));
        domainErrors.AddRange(ValidateProviderRef(csvRecord));
        domainErrors.AddRange(ValidateEPAOrgId(csvRecord));
        domainErrors.AddRange(ValidateReservation(csvRecord, reservationValidationResults));

        domainErrors.AddRange(ValidateRecognisePriorLearning(csvRecord));
        
        var minimumOffTheJobTrainingHoursForCourse = GetCourseSpecificMinimumOtjHours(csvRecord.CourseCode, otjTrainingHours);
        domainErrors.AddRange(ValidateTrainingTotalHours(csvRecord, minimumOffTheJobTrainingHoursForCourse));
        domainErrors.AddRange(ValidateTrainingHoursReduction(csvRecord, rplConfig.MaximumTrainingTimeReduction, minimumOffTheJobTrainingHoursForCourse));
        domainErrors.AddRange(ValidateDurationReducedBy(csvRecord));
        domainErrors.AddRange(ValidatePriceReducedBy(csvRecord, rplConfig.MinimumPriceReduction));
       
        return domainErrors;
    }
    
    private static int GetCourseSpecificMinimumOtjHours(string courseCode, Dictionary<string, int?> otjTrainingHours)
    {
        if (otjTrainingHours != null && otjTrainingHours.TryGetValue(courseCode, out var courseSpecificHours) && courseSpecificHours.HasValue)
        {
            return courseSpecificHours.Value;
        }
        
        return 187;
    }

    private async Task<string> GetEmployerName(string agreementId)
    {
        var employerDetails = await GetEmployerDetails(agreementId);
        return employerDetails.Name;
    }

    private async Task<EmployerSummary> GetEmployerDetails(string agreementId)
    {
        if (string.IsNullOrEmpty(agreementId))
        {
            return new EmployerSummary(agreementId, null, null, string.Empty, null, string.Empty);
        }
        
        if (_employerSummaries.ContainsKey(agreementId))
        {
            var result = _employerSummaries.GetValueOrDefault(agreementId);
            return result;
        }

        var accountLegalEntity = dbContext.Value.AccountLegalEntities
            .Include(x => x.Account)
            .FirstOrDefault(x => x.PublicHashedId == agreementId);

        if (accountLegalEntity == null)
        {
            return new EmployerSummary(agreementId, null, null, string.Empty, null, string.Empty);
        }
            
        var employerName = accountLegalEntity.Account.Name;
        var isLevy = accountLegalEntity.Account.LevyStatus == Types.ApprenticeshipEmployerType.Levy;
        var isSigned = await employerAgreementService.IsAgreementSigned(accountLegalEntity.AccountId, accountLegalEntity.MaLegalEntityId);
        var employerSummary = new EmployerSummary(agreementId, accountLegalEntity.Id, isLevy, employerName, isSigned, accountLegalEntity.LegalEntityId);
        
        _employerSummaries.Add(employerSummary);
        
        return employerSummary;
    }

    private Cohort GetCohortDetails(string cohortRef)
    {
        if (_cachedCohortDetails.ContainsKey(cohortRef))
        {
            return _cachedCohortDetails.GetValueOrDefault(cohortRef);
        }

        var cohort = dbContext.Value.Cohorts
            .Include(x => x.AccountLegalEntity)
            .Include(x => x.Apprenticeships).FirstOrDefault(x => x.Reference == cohortRef);
        
        _cachedCohortDetails.Add(cohortRef, cohort);

        return cohort;
    }

    private Standard GetStandardDetails(string stdCode)
    {
        if (string.IsNullOrWhiteSpace(stdCode))
        {
            return null;
        }

        return int.TryParse(stdCode, out var result) ? dbContext.Value.Standards.FirstOrDefault(x => x.LarsCode == result) : null;
    }
}