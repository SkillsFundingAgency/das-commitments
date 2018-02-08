namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public enum ApprenticeshipChangeType
    {
        Created = 0,
        Updated = 1,
        ChangeOfStatus = 2,
        ApprovingChange = 3,
        DataLockFailureOccured = 4,
        DataLockFailureResolved = 5,
        ChangeOfStopDate = 6
    }
}