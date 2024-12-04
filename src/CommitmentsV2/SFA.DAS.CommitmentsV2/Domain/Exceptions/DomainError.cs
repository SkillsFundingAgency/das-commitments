namespace SFA.DAS.CommitmentsV2.Domain.Exceptions;

public class DomainError
{
    public DomainError(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    public string PropertyName { get; }
    public string ErrorMessage { get; }
}