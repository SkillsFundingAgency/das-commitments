using System;

namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationValidationMessage
    {
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseCode { get; set; }
    }
}