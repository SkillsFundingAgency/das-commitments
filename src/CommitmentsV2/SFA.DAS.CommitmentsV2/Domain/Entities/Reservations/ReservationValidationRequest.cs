using System;

namespace SFA.DAS.CommitmentsV2.Domain.Entities.Reservations
{
    public class ReservationValidationRequest
    {
        public long AccountId { get; }
        public Guid ReservationId { get; }
        public DateTime StartDate { get; }
        public string CourseCode { get; }

        public ReservationValidationRequest(long accountId, Guid reservationId, DateTime startDate, string courseCode)
        {
            AccountId = accountId;
            ReservationId = reservationId;
            StartDate = startDate;
            CourseCode = courseCode;
        }
    }
}
