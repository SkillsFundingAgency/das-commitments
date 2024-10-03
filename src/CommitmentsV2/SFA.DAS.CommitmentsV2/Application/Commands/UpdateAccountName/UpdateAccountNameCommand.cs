namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;

public class UpdateAccountNameCommand : IRequest
{
    public long AccountId { get; }
    public string Name { get; }
    public DateTime Created { get; }

    public UpdateAccountNameCommand(long accountId, string name, DateTime created)
    {
        AccountId = accountId;
        Name = name;
        Created = created;
    }
}