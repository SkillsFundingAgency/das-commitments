using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Reservations.Api.Types
{
    public class BulkCreateReservationsWithNoneLevyResult
    {
        public List<BulkCreateReservationResult> BulkCreateResults { get; set; }
    }

    public class BulkCreateReservationResult
    {
        public string ULN { get; set; }
        public Guid ReservationId { get; set; }
    }

}
