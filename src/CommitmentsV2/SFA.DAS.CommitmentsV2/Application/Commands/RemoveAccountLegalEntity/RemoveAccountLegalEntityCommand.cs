namespace SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;

public class RemoveAccountLegalEntityCommand(long accountId, long accountLegalEntityId, DateTime? removed) : IRequest
{
    public long AccountId { get; } = accountId;
    public long AccountLegalEntityId { get; } = accountLegalEntityId;
    public DateTime? Removed { get; } = removed;
}