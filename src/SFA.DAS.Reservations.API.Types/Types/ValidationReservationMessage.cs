using System;

namespace SFA.DAS.Reservations.Api.Types.Types
{
    public class ValidationReservationMessage
    {
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseCode { get; set; }
    }
}