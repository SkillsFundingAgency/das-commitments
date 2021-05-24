using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class BulkUploadValidatorRequest :SaveDataRequest
    {
        public int MyProperty { get; set; }

        public IEnumerable<BulkUploadApprenticeship> BulkUploadApprenticeships { get; set; }
    }

    public class BulkUploadApprenticeship
    {
        public long ProviderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string ULN { get; set; }
        public ProgrammeType CourseType { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public AgreementStatus AgreementStatus { get; set; }
        public string ProviderRef { get; set; }
        public string EmployerRef { get; set; }
        public int? ProgType { get; set; }
        public bool HasStarted { get; set; }
        public bool IsLockedForUpdate { get; set; }
        public bool IsPaidForByTransfer { get; set; }
        public bool IsUpdateLockedForStartDateAndCourse { get; set; }
        public bool IsEndDateLockedForUpdate { get; set; }
        public string StartDateTransfersMinDateAltDetailMessage { get; set; }
        public Guid? ReservationId { get; set; }
        public bool IsContinuation { get; set; }
        public long AccountId { get; set; }
        public long LegalEntityId { get; set; }
        public string OriginatorReference { get; set; }
        public string CohortRef { get; set; }
    }

    public class BulkUploadResponse
    {
        
        public List<BulkCreateResult> Results { get; set; }
    }

    public class BulkCreateResult
    {
        public long CohortId { get; set; }
        public string CohortReference { get; set; }
        public long LegalEntityId { get; set; }
        public string ApprenticeName { get; set; }
        public long ApprenticeshipId { get; set; }
        public string LegalEntityName { get; set; }
        public string AccountName { get; set; }
    }

    public class BulkCohortActionRequest : SaveDataRequest
    {
        public CohortAction CohortAction { get; set; }

        public List<long> CohortIds { get; set; }
    }

    public enum CohortAction
    {
        Approve = 0,
        Send = 1,
    }
}
