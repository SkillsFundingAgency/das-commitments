using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipUpdatedApprovedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime ApprovedOn { get; set; }
        public string ULN { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PriceEpisode[] PriceEpisodes { get; set; }
        public TrainingType TrainingType { get; set; }
        public string TrainingCode { get; set; }
    }
}
