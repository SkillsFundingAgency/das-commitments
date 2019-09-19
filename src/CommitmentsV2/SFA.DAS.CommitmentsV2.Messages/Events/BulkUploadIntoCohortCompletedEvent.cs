using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class BulkUploadIntoCohortCompletedEvent
    {
        public long ProviderId { get; set; }
        public long CohortId { get; set; }
        public uint NumberOfApprentices { get; set; }
        public DateTime UploadedOn { get; set; }
    }
}