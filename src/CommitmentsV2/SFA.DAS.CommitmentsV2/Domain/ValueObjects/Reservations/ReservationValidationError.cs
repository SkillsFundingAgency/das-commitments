namespace SFA.DAS.CommitmentsV2.Domain.ValueObjects.Reservations
{
    public class ReservationValidationError
    {
        public string PropertyName { get; }
        public string Reason { get; }
        public string Code { get; }

        public ReservationValidationError(string propertyName, string reason, string code)
        {
            PropertyName = propertyName;
            Reason = reason;
            Code = code;
        }
    }
}
