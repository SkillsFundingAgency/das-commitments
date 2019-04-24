namespace SFA.DAS.CommitmentsV2.Domain.ValueObjects.Reservations
{
    public class ReservationValidationError
    {
        public string PropertyName { get; private set; }
        public string Reason { get; private set; }
        public string Code { get; private set; }

        public ReservationValidationError(string propertyName, string reason, string code)
        {
            PropertyName = propertyName;
            Reason = reason;
            Code = code;
        }
    }
}
