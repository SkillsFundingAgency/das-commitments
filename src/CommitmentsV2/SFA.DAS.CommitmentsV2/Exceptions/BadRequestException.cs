namespace SFA.DAS.CommitmentsV2.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
        // just call base    
    }
}