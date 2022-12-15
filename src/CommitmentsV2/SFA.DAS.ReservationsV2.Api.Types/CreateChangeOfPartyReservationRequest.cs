namespace SFA.DAS.Reservations.Api.Types
{
    public class CreateChangeOfPartyReservationRequest
    {
        public long? AccountLegalEntityId { get; set; }
        public long? ProviderId { get; set; }
    }
}
