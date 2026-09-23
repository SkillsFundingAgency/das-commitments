using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IValidationService
{
    public Task<List<Error>> ValidateAgreementIdValidFormat(string agreementId, string employerName);
    public Task<List<Error>> ValidateAgreementIdIsSigned(bool? isSigned);
    public Task<List<Error>> ValidateCohortRef(BulkUploadAddDraftApprenticeshipRequest csvRecord, long providerId, Cohort cohort, string employerName);
    public Task<bool> ValidatePermissionToCreateCohort(long providerId, ICollection<Error> domainErrors, bool? isLevy, EmployerSummary employerDetails);
    public IEnumerable<Error> ValidateCost(string costAsString, int? cost);
    public IEnumerable<Error> ValidateDateOfBirth(BulkUploadAddDraftApprenticeshipRequest csvRecord, ProviderStandardResults providerStandardResults,Standard standard);
    public IEnumerable<Error> ValidateDurationReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateEmailAddress(BulkUploadAddDraftApprenticeshipRequest csvRecord, List<BulkUploadAddDraftApprenticeshipRequest> csvRecords);
    public IEnumerable<Error> ValidateEndDate(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateEPAOrgId(string EPAOrgId);
    public IEnumerable<Error> ValidateLastName(string lastName);
    public IEnumerable<Error> ValidateFirstName(string firstName);
    public IEnumerable<Error> ValidatePriceReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord, int minPriceReduction);
    public IEnumerable<Error> ValidateProviderRef(string providerRef);
    public IEnumerable<Error> ValidateRecognisePriorLearning(BulkUploadAddDraftApprenticeshipRequest csvRecord);
    public IEnumerable<Error> ValidateReservation(BulkUploadAddDraftApprenticeshipRequest csvRecord, BulkReservationValidationResults reservationValidationResults);
    public IEnumerable<Error> ValidateStartDate(BulkUploadAddDraftApprenticeshipRequest csvRecord, Standard standard, Cohort cohort);
    public IEnumerable<Error> ValidateTrainingHoursReduction(BulkUploadAddDraftApprenticeshipRequest csvRecord, int maxTrainingHoursReduction, int minimumOffTheJobTrainingHoursForCourse);
    public IEnumerable<Error> ValidateTrainingTotalHours(BulkUploadAddDraftApprenticeshipRequest csvRecord, int minimumOffTheJobTrainingHoursForCourse);
    public IEnumerable<Error> ValidateUln(BulkUploadAddDraftApprenticeshipRequest csvRecord, List<BulkUploadAddDraftApprenticeshipRequest> csvRecords);
    public List<Error> ValidateDeclaredStandards(ProviderStandardResults providerStandardResults);
    public IEnumerable<Error> ValidateCourseCode(string courseCode, ProviderStandardResults providerStandardResults, Standard standard);
}