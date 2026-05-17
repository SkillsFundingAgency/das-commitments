namespace SFA.DAS.CommitmentsV2.Models;

public class LearningChangeHistory
{
    public Guid Id { get; set; }
    public byte Source { get; set; }
    public byte ChangeType { get; set; }
    public string Description { get; set; }
    public long? UserId { get; set; }
    public long ApprenticeshipId { get; set; }
    public string LearnerName { get; set; }
    public Guid? LearnerKey { get; set; }
    public DateTime Created { get; set; }
    public DateTime AppliedDate { get; set; }
    public string AccountId { get; set; }
    public long UKPRN { get; set; }
    public string ProviderName { get; set; }
    public string EmployerName { get; set; }
}