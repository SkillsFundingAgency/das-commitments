namespace SFA.DAS.CommitmentsV2.Exceptions;

public class NullConnectionStringException(string message) : Exception(message)
{
    public NullConnectionStringException() : this("The connection string provided is null.") { }
}