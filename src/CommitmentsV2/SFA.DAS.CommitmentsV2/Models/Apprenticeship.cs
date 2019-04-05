using System;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using AgreementStatus = SFA.DAS.Commitments.Api.Types.AgreementStatus;

namespace SFA.DAS.CommitmentsV2.Models
{
    public abstract class Apprenticeship
    {
        public bool IsApproved { get; set; }

        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Uln { get; set; }
        public ProgrammeType? TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AgreementStatus AgreementStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NiNumber { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? PaymentOrder { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public byte? PendingUpdateOriginator { get; set; }
        public string EpaOrgId { get; set; }
        public long? CloneOf { get; set; }

        public Guid? ReservationId { get; set; }

        public virtual Commitment Commitment { get; set; }
        public virtual AssessmentOrganisation EpaOrg { get; set; }
    }
}
