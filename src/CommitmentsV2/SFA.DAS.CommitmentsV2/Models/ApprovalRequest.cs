namespace SFA.DAS.CommitmentsV2.Models;

public class ApprovalRequest
{
    public ApprovalRequest()
    {
        Items = new List<ApprovalFieldRequest>();
    }

    public Guid Id { get; set; }
    public DateTime Created { get; }
    public DateTime? Updated { get; set; }
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public CocLearningType LearningType { get; set; }
    public string UKPRN { get; set; }
    public string ULN { get; set; }
    public string Reason { get; set; }
    public CocApprovalResultStatus? Status { get; set; }
    public DateTime? ProviderAcknowledgedAt { get; set; }
    public string ProviderAcknowledgedBy { get; set; }
    public virtual ICollection<ApprovalFieldRequest> Items { get; set; }
    public virtual Apprenticeship Apprenticeship { get; set; }

    public void AcknowledgeByProvider(string userId, DateTime acknowledgedAt)
    {
        ProviderAcknowledgedAt = acknowledgedAt;
        ProviderAcknowledgedBy = userId;
    }
}