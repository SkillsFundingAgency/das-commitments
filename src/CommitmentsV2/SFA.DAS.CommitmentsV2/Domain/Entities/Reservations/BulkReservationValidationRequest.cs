namespace SFA.DAS.CommitmentsV2.Domain.Entities.Reservations
{
    public class BulkValidateRequest
    {
        public long AccountId { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseId { get; set; }
        public uint? ProviderId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public bool IsLevyAccount { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? TransferSenderAccountId { get; set; }
        public Guid? UserId { get; set; }
    }
}
