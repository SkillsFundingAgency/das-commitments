namespace SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;

public class ReservationValidationRequest
{
    public Guid ReservationId { get; }
    public DateTime StartDate { get; }
    public string CourseCode { get; }

    public ReservationValidationRequest(Guid reservationId, DateTime startDate, string courseCode)
    {
        ReservationId = reservationId;
        StartDate = startDate;
        CourseCode = courseCode;
    }
}