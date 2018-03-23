namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public enum CommitmentChangeType
    {
        Created = 0,
        Deleted = 1,
        CreatedApprenticeship = 2,
        DeletedApprenticeship = 3,
        EditedApprenticeship = 4,
        SentForReview = 5,
        SentForApproval = 6,
        FinalApproval = 7,
        BulkUploadedApprenticeships = 8,
        TransferSenderApproval = 9
    }
}