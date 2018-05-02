using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities
{
    public class DbSetupApprenticeship : IDbSetupEntity
    {
        public long Id { get; set; }
        public long CommitmentId { get; set; }
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        [RegularExpression(@"[1-9]\d{9}")] //todo: better generation?
        public string ULN { get; set; }
        public TrainingType? TrainingType { get; set; }
        [StringLength(20)]
        public string TrainingCode { get; set; }
        [StringLength(126)]
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AgreementStatus AgreementStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [RegularExpression("^[A-CEGHJ-PR-TW-Z]{1}[A-CEGHJ-NPR-TW-Z]{1}[0-9]{6}[A-DFM]{0,1}$")]
        public string NINumber { get; set; }
        [StringLength(50)]
        public string EmployerRef { get; set; }
        [StringLength(50)]
        public string ProviderRef { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? AgreedOn { get; set; }
        public int PaymentOrder { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public Originator? PendingOriginator { get; set; }
        [StringLength(50)]
        // todo: depends whether we get merged before EPA PR's
        public string EPAOrgId { get; set; }
    }
}
