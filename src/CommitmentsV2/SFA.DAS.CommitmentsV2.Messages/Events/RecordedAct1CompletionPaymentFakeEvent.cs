using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class RecordedAct1CompletionPaymentFakeEvent
    {
        public DateTimeOffset EventTime { get; set; }
        public long? ApprenticeshipId { get; set; }
    }
}
