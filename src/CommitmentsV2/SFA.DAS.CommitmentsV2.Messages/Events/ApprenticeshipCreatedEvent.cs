using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipCreatedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Uln { get; set; }
        public long ProviderId { get; set; }
        public long AccountId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string LegalEntityName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PriceEpisode[] PriceEpisodes { get; set; }
        public ProgrammeType TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public long? TransferSenderId { get; set; }
    }
}
