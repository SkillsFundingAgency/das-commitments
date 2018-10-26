using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock;

namespace SFA.DAS.Commitments.Api.Types.ApprovedApprenticeship
{
    public class ApprovedApprenticeship
    {
        public ApprovedApprenticeship()
        {
            DataLocks = new List<DataLockStatus>();
            PriceEpisodes = new List<PriceHistory>();
        }

        public long Id { get; set; }
        public string CohortReference { get; set; }
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public long? TransferSenderId { get; set; }
        public string Reference { get; set; }
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
    }
}
