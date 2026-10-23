namespace SFA.DAS.CommitmentsV2.Models;

public class CocApprovalItem
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public CocLearningType LearningType { get; set; }
    public string UKPRN { get; set; }
    public string ULN { get; set; }
    public string Field { get; set; }
    public string Old { get; set; }
    public string New { get; set; }
    public CocApprovalItemStatus? Status { get; set; }
    public string Reason { get; set; }
    public DateTime? ProviderAcknowledgedAt { get; set; }
    public string ProviderAcknowledgedBy { get; set; }
    public DateTime? EmployerAcknowledgedAt { get; set; }
    public string EmployerAcknowledgedBy { get; set; }
}