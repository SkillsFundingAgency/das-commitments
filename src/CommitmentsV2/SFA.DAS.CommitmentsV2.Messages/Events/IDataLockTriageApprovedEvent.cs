using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public interface IDataLockTriageApprovedEvent
    {
        long ApprenticeshipId { get; set; }
        DateTime ApprovedOn { get; set; }
        PriceEpisode[] PriceEpisodes { get; set; }
        TrainingType TrainingType { get; set; }
        string TrainingCode { get; set; }
    }
}
