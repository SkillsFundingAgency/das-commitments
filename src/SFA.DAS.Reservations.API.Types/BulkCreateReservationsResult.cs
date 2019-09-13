using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Reservations.Api.Types
{
    public class BulkCreateReservationsResult
    {
        public BulkCreateReservationsResult(IEnumerable<Guid> reservationIds)
        {
            ReservationIds = reservationIds.ToArray();
        }
        public Guid[] ReservationIds { get; }
    }
}