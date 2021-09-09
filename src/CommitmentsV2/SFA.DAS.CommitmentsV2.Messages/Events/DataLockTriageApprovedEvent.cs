using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class DataLockTriageApprovedEvent
    {
        public long ApprenticeshipId { get; set; }
        public string StandardUId { get; set; }
        public string TrainingCourseVersion { get; set; }
        public DateTime ApprovedOn { get; set; }
        public PriceEpisode[] PriceEpisodes { get; set; }
        public ProgrammeType TrainingType { get; set; }
        public string TrainingCode { get; set; }
    }
}
