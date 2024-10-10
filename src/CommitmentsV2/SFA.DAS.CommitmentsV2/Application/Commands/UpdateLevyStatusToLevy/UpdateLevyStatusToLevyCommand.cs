namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy;

public class UpdateLevyStatusToLevyCommand : IRequest
{
    public long AccountId { get; set; }
}