namespace SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;

public class ReservationValidationResult
{
    public bool HasErrors => ValidationErrors.Length > 0;
    public bool IsValid => !HasErrors;
    public ReservationValidationError[] ValidationErrors { get; }

    public ReservationValidationResult(ReservationValidationError[] validationErrors)
    {
        ValidationErrors = validationErrors;
    }
}