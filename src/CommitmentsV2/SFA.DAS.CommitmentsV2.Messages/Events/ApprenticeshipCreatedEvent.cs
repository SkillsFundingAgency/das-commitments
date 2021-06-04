using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipCreatedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime AgreedOn { get; set; }  // Date agreed between Provider and Employer
        public DateTime CreatedOn { get; set; } // This is either the AgreedOn date or the date the TransferSender approved the request (if involved)
        public string Uln { get; set; }
        public long ProviderId { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string LegalEntityName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PriceEpisode[] PriceEpisodes { get; set; }
        public ProgrammeType TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public long? TransferSenderId { get; set; }
        public ApprenticeshipEmployerType? ApprenticeshipEmployerTypeOnApproval { get; set; }
        public long? ContinuationOfId { get; set; }
    }
}
