using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Domain.Entities.ApprovedApprenticeship
{
    public class ApprovedApprenticeship
    {
        public ApprovedApprenticeship()
        {
            PriceEpisodes = new List<PriceHistory>();
            DataLocks = new List<DataLockStatus>();
        }

        public long Id { get; set; }
        public string CohortReference { get; set; }
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public long? TransferSenderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ULN { get; set; }
        public TrainingType TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public DateTime? StopDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? AgreedOn { get; set; }
        public int PaymentOrder { get; set; }
        public Originator? UpdateOriginator { get; set; }
        public string ProviderName { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public string EndpointAssessorName { get; set; }
        public List<PriceHistory> PriceEpisodes { get; set; }
        public List<DataLockStatus> DataLocks { get; set; }

        public ApprovedApprenticeship Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ApprovedApprenticeship>(json);
        }
    }
}
