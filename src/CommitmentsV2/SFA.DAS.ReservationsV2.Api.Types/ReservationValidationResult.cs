namespace SFA.DAS.Reservations.Api.Types;

public class ReservationValidationResult
{
    public ReservationValidationError[] ValidationErrors { get; set; } = [];
}