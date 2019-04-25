using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class DataLockTriageApprovedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime ApprovedOn { get; set; }
        public PriceEpisode[] PriceEpisodes { get; set; }
        public TrainingType TrainingType { get; set; }
        public string TrainingCode { get; set; }
    }
}
