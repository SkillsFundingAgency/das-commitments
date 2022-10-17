namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationValidationError
    {
        public ReservationValidationError(string propertyName, string reason)
        {
            PropertyName = propertyName;
            Reason = reason;
        }

        public ReservationValidationError()
        {
                    
        }

        public string PropertyName { get; set; }
        public string Reason { get; set; }
    }
}