using System;

namespace SFA.DAS.Reservations.Api.Types
{
    public class CreateChangeOfPartyReservationResult
    {
        public CreateChangeOfPartyReservationResult(Guid reservationId)
        {
            ReservationId = reservationId;
        }

        public Guid ReservationId { get; }
    }
}
