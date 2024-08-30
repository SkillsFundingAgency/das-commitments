using System;

namespace SFA.DAS.Reservations.Api.Types
{
    public class CreateAutoReservationRequest
    {
        public Guid Id => Guid.NewGuid();
        public long AccountId { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseId { get; set; }
        public uint? ProviderId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public bool IsLevyAccount => false;
        public long? TransferSenderAccountId => null;
        public Guid? UserId { get; set; }
    }
}