using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipUpdatedApprovedEvent
    {
        public long ApprenticeshipId { get; set; }
        public string StandardUId { get; set; }
        public string TrainingCourseVersion { get; set; }
        public string TrainingCourseOption { get; set; }
        public DateTime ApprovedOn { get; set; }
        public string Uln { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PriceEpisode[] PriceEpisodes { get; set; }
        public ProgrammeType TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public DeliveryModel DeliveryModel { get; set; }
    }
}
