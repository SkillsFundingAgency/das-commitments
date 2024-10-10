using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public class SupportApprenticeshipDetails
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Uln { get; set; }
    public string EmployerName { get; set; }
    public string ProviderName { get; set; }
    public long ProviderId { get; set; }
    public string CourseName { get; set; }
    public DeliveryModel DeliveryModel { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PauseDate { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public ApprenticeshipStatus ApprenticeshipStatus { get; set; }
    public IEnumerable<Alerts> Alerts { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string ProviderRef { get; set; }
    public string EmployerRef { get; set; }
    public decimal? TotalAgreedPrice { get; set; }
    public string CohortReference { get; set; }
    public long AccountLegalEntityId { get; set; }
    public ConfirmationStatus? ConfirmationStatus { get; set; }
    public long? TransferSenderId { get; set; }
    public bool HasHadDataLockSuccess { get; set; }
    public string CourseCode { get; set; }
    public decimal? Cost { get; set; }
    public AgreementStatus AgreementStatus { get; set; }
    public DateTime? StopDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool? MadeRedundant { get; set; }
    public string StandardUId { get; set; }
    public string TrainingCourseVersion { get; set; }
    public bool TrainingCourseVersionConfirmed { get; set; }
    public string TrainingCourseOption { get; set; }
    public long EmployerAccountId { get; internal set; }
    public int? EmploymentPrice { get; set; }
    public DateTime? EmploymentEndDate { get; set; }
}