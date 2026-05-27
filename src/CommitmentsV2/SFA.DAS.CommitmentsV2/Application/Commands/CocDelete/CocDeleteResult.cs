namespace SFA.DAS.CommitmentsV2.Application.Commands.CocDelete;

public class CocDeleteResult
{
    public string Message { get; set; }

    public DeleteValidationState Status { get; set; }
}

public enum DeleteValidationState
{
    NotFound = 404,
    NotPending = 400,
    Cancelled = 200
}