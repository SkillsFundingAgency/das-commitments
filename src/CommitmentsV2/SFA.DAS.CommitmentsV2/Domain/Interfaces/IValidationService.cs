using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IValidationService
{
    public Task<List<Error>> ValidateAgreementIdValidFormat(BulkUploadAddDraftApprenticeshipRequest csvRecord, string employerName);
    public Task<List<Error>> ValidateAgreementIdIsSigned(BulkUploadAddDraftApprenticeshipRequest csvRecord, bool? isSigned);
    public Task<List<Error>> ValidateCohortRef(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId, Cohort cohort);
    public Task<bool> ValidatePermissionToCreateCohort(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId, ICollection<Error> domainErrors, bool? isLevy);
    public IEnumerable<Error> ValidateCost(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateDateOfBirth(BulkUploadAddDraftApprenticeshipRequest csvRecord, ProviderStandardResults providerStandardResults);
    public IEnumerable<Error> ValidateDurationReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateEmailAddress(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateEndDate(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateEPAOrgId(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateFamilyName(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateGivenName(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidatePriceReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord, int minPriceReduction);
    public IEnumerable<Error> ValidateProviderRef(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateRecognisePriorLearning(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateReservation(BulkUploadAddDraftApprenticeshipRequest csvRecord, BulkReservationValidationResults reservationValidationResults);
    public IEnumerable<Error> ValidateStartDate(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateTrainingHoursReduction(BulkUploadAddDraftApprenticeshipRequest csvRecord, int maxTrainingHoursReduction, int minimumOffTheJobTrainingHoursForCourse);
    public IEnumerable<Error> ValidateTrainingTotalHours(BulkUploadAddDraftApprenticeshipRequest csvRecord, int minimumOffTheJobTrainingHoursForCourse);
    public IEnumerable<Error> ValidateUln(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public List<Error> ValidateDeclaredStandards(ProviderStandardResults providerStandardResults);
    public IEnumerable<Error> ValidateCourseCode(BulkUploadAddDraftApprenticeshipRequest csvRecord, ProviderStandardResults providerStandardResults);

}