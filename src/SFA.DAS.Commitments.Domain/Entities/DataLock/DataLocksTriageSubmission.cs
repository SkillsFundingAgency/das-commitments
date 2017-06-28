namespace SFA.DAS.Commitments.Domain.Entities.DataLock
{
    public class DataLocksTriageSubmission
    {
        public TriageStatus TriageStatus { get; set; }
        public string UserId { get; set; }
    }

    public class DataLocksTriageResolutionSubmission
    {
        public DataLockUpdateType DataLockUpdateType { get; set; }
        public TriageStatus TriageStatus { get; set; }
        public string UserId { get; set; }
    }

    public enum DataLockUpdateType
    {
        ApproveChanges = 0,
        RejectChanges = 1
    }
}