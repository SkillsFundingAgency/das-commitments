using System;

namespace SFA.DAS.Reservations.Api.Types
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseId { get; set; }
        public uint? ProviderId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public bool IsLevyAccount { get; set; }
        public long? TransferSenderAccountId { get; set; }
        public Guid? UserId { get; set; }
    }
}
