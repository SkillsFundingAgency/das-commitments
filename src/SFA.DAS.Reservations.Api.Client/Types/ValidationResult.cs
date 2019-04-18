namespace SFA.DAS.Reservations.Api.Client.Types
{
    public class ValidationResult
    {
        public bool HasErrors => ValidationErrors.Length > 0;
        public bool IsOkay => !HasErrors;
        public ValidationError[] ValidationErrors { get; set; }
    }
}