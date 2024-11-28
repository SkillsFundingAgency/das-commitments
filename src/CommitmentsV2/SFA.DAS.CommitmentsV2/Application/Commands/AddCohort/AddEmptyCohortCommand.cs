using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

public class AddEmptyCohortCommand : IRequest<AddCohortResult>
{
    public long AccountId { get; }
    public long AccountLegalEntityId { get; }
    public long ProviderId { get; }

    public UserInfo UserInfo { get; }

    public AddEmptyCohortCommand(long accountId, long accountLegalEntityId, long providerId, UserInfo userInfo)
    {
        AccountId = accountId;
        AccountLegalEntityId = accountLegalEntityId;
        ProviderId = providerId;
        UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
    }
}