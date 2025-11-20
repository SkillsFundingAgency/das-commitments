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

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler(
    ILogger<ValidateLearnerCommandHandler> logger,
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IOverlapCheckService overlapService,
    IAcademicYearDateProvider academicYearDateProvider,
    IProviderRelationshipsApiClient providerRelationshipsApiClient,
    IEmployerAgreementService employerAgreementService,
    RplSettingsConfiguration rplConfig,
    IUlnValidator ulnValidator,
    ILinkGenerator urlHelper)
    : IRequestHandler<ValidateLearnerCommand, LearnerValidateApiResponse>
{
    public long ProviderId { get; set; }
    public long LearnerDataId { get; set; }

    public async Task<LearnerValidateApiResponse> Handle(ValidateLearnerCommand command, CancellationToken cancellationToken)
    {
        ProviderId = command.ProviderId;
        LearnerDataId = command.LearnerDataId;

        var criticalErrors = ValidateDeclaredStandards(command.ProviderStandardsData);
        criticalErrors.AddRange(await ValidateCriticalErrors(command.LearnerData, command.ProviderId));

        //if (criticalErrors.Count != 0)
        //{
        //    return new LearnerValidateApiResponse
        //    {
        //        CriticalErrors = criticalErrors
        //    };
        //}

        var errors = await Validate(command.LearnerData, command.ProviderId, command.LearnerDataId, command.ProviderStandardsData, command.OtjTrainingHours);

        return new LearnerValidateApiResponse
        {
            CriticalErrors = criticalErrors,
            LearnerValidation = new LearnerValidation(command.LearnerDataId, errors)
        };

    }

    //private async Task AddError(List<BulkUploadValidationError> bulkUploadValidationErrors, BulkUploadAddDraftApprenticeshipRequest csvRecord, List<Error> domainErrors)
    //{
    //    if (domainErrors.Count != 0)
    //    {
    //        bulkUploadValidationErrors.Add(new BulkUploadValidationError(
    //            csvRecord.RowNumber,
    //            await GetEmployerName(csvRecord.AgreementId),
    //            csvRecord.Uln,
    //            csvRecord.FirstName + " " + csvRecord.LastName,
    //            domainErrors
    //        ));
    //    }
    //}

    private async Task<List<LearnerError>> ValidateCriticalErrors(LearnerDataEnhanced record, long providerId)
    {
        var domainErrors = await ValidateAgreementIdValidFormat(record);
        if (domainErrors.Count == 0)
        {
            domainErrors.AddRange(await ValidateAgreementIdIsSigned(record));

            // when a valid agreement has not been signed validation will stop
            if (domainErrors.Count != 0)
            {
                return domainErrors;
            }
        }

        return domainErrors;
    }

    private async Task<List<LearnerError>> Validate(LearnerDataEnhanced learner, long providerId, long learnerDataId, ProviderStandardResults providerStandardResults, int? otjTrainingHours)
    {
        var errors = new List<LearnerError>();

        //if (domainErrors.Count == 0)
        //{
        //    domainErrors.AddRange(await ValidateAgreementIdIsSigned(csvRecord));

        //    // when a valid agreement has not been signed validation will stop
        //    if (domainErrors.Count != 0)
        //    {
        //        return domainErrors;
        //    }
        //}

        errors.AddRange(await ValidateUln(learner));
        errors.AddRange(ValidateFamilyName(learner));
        errors.AddRange(ValidateGivenName(learner));
        errors.AddRange(ValidateDateOfBirth(learner));
        errors.AddRange(ValidateEmailAddress(learner));
        errors.AddRange(ValidateCourseCode(learner, providerStandardResults));
        errors.AddRange(ValidateStartDate(learner));
        errors.AddRange(ValidateEndDate(learner));
        errors.AddRange(ValidateCost(learner));
        //errors.AddRange(ValidateProviderRef(csvRecord));
        //errors.AddRange(ValidateEPAOrgId(csvRecord));

        //wrrors.AddRange(ValidateRecognisePriorLearning(csvRecord));

        //var minimumOffTheJobTrainingHoursForCourse = GetCourseSpecificMinimumOtjHours(record.StandardCode, otjTrainingHours);
        //domainErrors.AddRange(ValidateTrainingTotalHours(csvRecord, minimumOffTheJobTrainingHoursForCourse));
        //domainErrors.AddRange(ValidateTrainingHoursReduction(csvRecord, rplConfig.MaximumTrainingTimeReduction, minimumOffTheJobTrainingHoursForCourse));
        //domainErrors.AddRange(ValidateDurationReducedBy(csvRecord));
        //domainErrors.AddRange(ValidatePriceReducedBy(csvRecord, rplConfig.MinimumPriceReduction));

        return errors;
    }

    //private static int GetCourseSpecificMinimumOtjHours(string courseCode, Dictionary<string, int?> otjTrainingHours)
    //{
    //    if (otjTrainingHours != null && otjTrainingHours.TryGetValue(courseCode, out var courseSpecificHours) && courseSpecificHours.HasValue)
    //    {
    //        return courseSpecificHours.Value;
    //    }

    //    return 187;
    //}

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

        //if (_employerSummaries.ContainsKey(agreementId))
        //{
        //    var result = _employerSummaries.GetValueOrDefault(agreementId);
        //    return result;
        //}

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

        return employerSummary;
    }

    //private Cohort GetCohortDetails(string cohortRef)
    //{
    //    if (_cachedCohortDetails.ContainsKey(cohortRef))
    //    {
    //        return _cachedCohortDetails.GetValueOrDefault(cohortRef);
    //    }

    //    var cohort = dbContext.Value.Cohorts
    //        .Include(x => x.AccountLegalEntity)
    //        .Include(x => x.Apprenticeships).FirstOrDefault(x => x.Reference == cohortRef);

    //    _cachedCohortDetails.Add(cohortRef, cohort);

    //    return cohort;
    //}

    private Standard GetStandardDetails(string stdCode)
    {
        if (string.IsNullOrWhiteSpace(stdCode))
        {
            return null;
        }

        return int.TryParse(stdCode, out var result) ? dbContext.Value.Standards.FirstOrDefault(x => x.LarsCode == result) : null;
    }
}