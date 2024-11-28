namespace SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;

public class ReservationValidationError
{
    public string PropertyName { get; }
    public string Reason { get; }

    public ReservationValidationError(string propertyName, string reason)
    {
        PropertyName = propertyName;
        Reason = reason;
    }
}