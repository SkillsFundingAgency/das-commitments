namespace SFA.DAS.CommitmentsV2.Exceptions;

public class PendingApprovalNotFoundException : Exception
{
    public PendingApprovalNotFoundException() : base()
    {
    }

    public PendingApprovalNotFoundException(string message) : base(message)
    {
    }

    public PendingApprovalNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}