namespace SFA.DAS.CommitmentsV2.Types
{
    public enum ApprenticeshipUpdateStatus : byte
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Deleted = 3,
        Superceded = 4,
        Expired = 5
    }
}
