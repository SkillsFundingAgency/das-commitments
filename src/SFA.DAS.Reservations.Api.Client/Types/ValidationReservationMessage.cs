using System;

namespace SFA.DAS.Reservations.Api.Client.Types
{
    public class ValidationReservationMessage
    {
        public long AccountId { get; set; }
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseCode { get; set; }
    }
}