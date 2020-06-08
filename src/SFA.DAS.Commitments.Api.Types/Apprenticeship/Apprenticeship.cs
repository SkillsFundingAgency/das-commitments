using System;

using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.Types.Apprenticeship
{
    public class Apprenticeship
    {
        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public long? TransferSenderId { get; set; }
        public string Reference { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NINumber { get; set; }
        public string ULN { get; set; }
        public TrainingType TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? StopDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public AgreementStatus AgreementStatus { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public bool CanBeApproved { get; set; }
        public Originator? PendingUpdateOriginator { get; set; }
        public string ProviderName { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }

        public bool DataLockPrice { get; set; }
        public bool DataLockPriceTriaged { get; set; }
        public bool DataLockCourse { get; set; }
        public bool DataLockCourseTriaged { get; set; }
        public bool DataLockCourseChangeTriaged { get; set; }
        public bool DataLockTriagedAsRestart { get; set; }
        public bool HasHadDataLockSuccess { get; set; }

        public string ApprenticeshipName => $"{FirstName} {LastName}";

        public string EndpointAssessorName { get; set; }
        public Guid? ReservationId { get; set; }
        public long? ContinuationOfId { get; set; }
        public DateTime? PreviousApprenticeshipStopDate { get; set; }
    }
}