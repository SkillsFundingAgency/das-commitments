using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities
{
    public class DbSetupApprenticeship
    {
        // identity column -> 
        public long Id { get; set; }
        public long CommitmentId { get; set; }
        //public long EmployerAccountId { get; set; }
        //public long ProviderId { get; set; }
        //todo: public string Reference { get; set; }
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        //[StringLength(50)]
        //[Range(1000000000,9999999999)]
        //[RegularExpression("[1-9]{1}[0-9]{9}")]
        //        [RegularExpression("[1-9][0-9]")]//[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]")]
        [RegularExpression(@"[1-9]\d{9}")] //todo: better generation
        public string ULN { get; set; }
        //public TrainingType TrainingType { get; set; }
        //[Range(TrainingType.Standard, TrainingType.Framework)]
        //https://stackoverflow.com/questions/36850313/random-enum-generation
        //https://stackoverflow.com/questions/20957010/create-anonymous-enum-value-from-a-subset-of-all-values
        //[EnumDataType(typeof(TrainingType))]
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
        //[StringLength(10)]
        //[RegularExpression(@"^(?!BG)(?!GB)(?!NK)(?!KN)(?!TN)(?!NT)(?!ZZ)(?:[A-CEGHJ-PR-TW-Z][A-CEGHJ-NPR-TW-Z])(?:\s*\d\s*){6}([A-D]|\s)$")]
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
        //public PaymentStatus PaymentStatus { get; set; }
        //public AgreementStatus AgreementStatus { get; set; }
        //public bool CanBeApproved { get; set; }
        //public Originator? PendingUpdateOriginator { get; set; }
        //public string ProviderName { get; set; }
        //public string LegalEntityId { get; set; }
        //public string LegalEntityName { get; set; }

        //public bool DataLockPrice { get; set; }
        //public bool DataLockPriceTriaged { get; set; }
        //public bool DataLockCourse { get; set; }
        //public bool DataLockCourseTriaged { get; set; }
        //public bool DataLockCourseChangeTriaged { get; set; }
        //public bool DataLockTriagedAsRestart { get; set; }
        public bool HasHadDataLockSuccess { get; set; }

        //public string ApprenticeshipName => $"{FirstName} {LastName}";

        [StringLength(50)]
        public string EPAOrgId { get; set; }
    }
}
