namespace SFA.DAS.Reservations.Api.Client.Types
{
    public class ValidationError
    {
        public string PropertyName { get; set; }
        public string Reason { get; set; }
        public string Code { get; set; }
    }
}