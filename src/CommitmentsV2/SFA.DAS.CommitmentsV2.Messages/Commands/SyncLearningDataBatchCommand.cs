using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    /// <summary>
    /// SyncLearningDataBatchCommand allows for batch processing of sync workload
    /// </summary>
    public class SyncLearningDataBatchCommand
    {
        public long[] Ids { get; set; }
    }

    /// <summary>
    /// SyncLearningCommand is for consumption in Learning
    /// </summary>
    /// <param name="innerEvent"></param>
    public class SyncLearningCommand(ApprenticeshipCreatedEvent innerEvent)
    {
        public ApprenticeshipCreatedEvent InnerEvent { get; set; } = innerEvent;
    }
}
