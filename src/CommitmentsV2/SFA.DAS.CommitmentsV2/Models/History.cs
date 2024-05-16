namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class History
    {
        public long Id { get; set; }
        public string EntityType { get; set; }
        public long? EntityId { get; set; }
        public long? CommitmentId { get; set; }
        public long? ApprenticeshipId { get; set; }
        public string UserId { get; set; }
        public string UpdatedByRole { get; set; }
        public string ChangeType { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? ProviderId { get; set; }
        public long? EmployerAccountId { get; set; }
        public string UpdatedByName { get; set; }
        public string OriginalState { get; set; }
        public string UpdatedState { get; set; }
        public string Diff { get; set; }
        public Guid? CorrelationId { get; set; }
    }
}
