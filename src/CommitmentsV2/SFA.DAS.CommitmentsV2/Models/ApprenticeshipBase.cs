using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public abstract class ApprenticeshipBase : Aggregate
    {
        protected ApprenticeshipBase()
        {
            ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest>();
        }

        public bool IsApproved { get; set; }
        public virtual long Id { get; set; }
        public virtual long CommitmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool? EmailAddressConfirmed { get; set; }
        public string Uln { get; set; }
        public ProgrammeType? ProgrammeType { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string TrainingCourseVersion { get; set; }
        public bool TrainingCourseVersionConfirmed { get; set; }
        public string TrainingCourseOption { get; set; }
        public string StandardUId { get; set; }
        public virtual DeliveryModel? DeliveryModel { get; set; }
        public decimal? Cost { get; set; }
        public int? TrainingPrice { get; set; }
        public int? EndPointAssessmentPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NiNumber { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? AgreedOn { get; set; }
        public string EpaOrgId { get; set; }
        public long? CloneOf { get; set; }
        public bool IsProviderSearch { get; set; }
        public Guid? ReservationId { get; set; }
        public long? ContinuationOfId { get; set; }
        public DateTime? OriginalStartDate { get; set; }

        public virtual Cohort Cohort { get; set; }
        public virtual AssessmentOrganisation EpaOrg { get; set; }

        public ApprenticeshipStatus ApprenticeshipStatus { get; set; }

        public virtual ICollection<ApprenticeshipUpdate> ApprenticeshipUpdate { get; set; }

        public bool IsContinuation => ContinuationOfId.HasValue;
        public virtual Apprenticeship PreviousApprenticeship { get; set; }

        public virtual ApprenticeshipConfirmationStatus ApprenticeshipConfirmationStatus { get; set; }
        public FlexibleEmployment FlexibleEmployment { get; set; }
        public bool? RecognisePriorLearning { get; set; }
        public ApprenticeshipPriorLearning PriorLearning { get; set; }
        public virtual ICollection<OverlappingTrainingDateRequest> OverlappingTrainingDateRequests { get; set; }
        public bool? IsOnFlexiPaymentPilot { get; set; }
        public int? TrainingTotalHours { get; set; }
        public bool? EmployerHasEditedCost { get; set; }
        public bool RecognisingPriorLearningExtendedStillNeedsToBeConsidered
        {
            get
            {
                if (StartDate >= Constants.RecognisePriorLearningBecomesRequiredOn)
                {
                    switch (RecognisePriorLearning)
                    {
                        case null:
                            return true;
                        case false:
                            return false;
                    }

                    if (TrainingTotalHours == null || PriorLearning?.DurationReducedByHours == null 
                          || PriorLearning?.IsDurationReducedByRpl == null || PriorLearning?.PriceReducedBy == null)
                    {
                        return true;
                    }

                    if (PriorLearning?.IsDurationReducedByRpl == true && PriorLearning?.DurationReducedBy == null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}