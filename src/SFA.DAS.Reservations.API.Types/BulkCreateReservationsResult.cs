using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Reservations.Api.Types
{
    public class BulkCreateReservationsResult
    {
        public BulkCreateReservationsResult(IEnumerable<Guid> reservations)
        {
            Reservations = reservations.ToArray();
        }
        public Guid[] Reservations { get; }
    }
}