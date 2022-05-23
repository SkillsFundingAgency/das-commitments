using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public abstract class ApprenticeshipBase : Aggregate
    {
        protected ApprenticeshipBase()
        {
            ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
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
        public DeliveryModel? DeliveryModel { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
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
    }
}