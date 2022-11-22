namespace SFA.DAS.Reservations.Api.Types
{
    public class BulkCreateReservationsRequest
    {
        public uint Count { get; set; }
        public long? TransferSenderId { get; set; }
    }
}