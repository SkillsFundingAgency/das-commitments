using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort;

public class ProcessFullyApprovedCohortCommand : IRequest
{
    public long CohortId { get; }
    public long AccountId { get; }
    public long? ChangeOfPartyRequestId { get; }
    public UserInfo UserInfo { get; }
    public Party LastApprovedBy { get; }

    public ProcessFullyApprovedCohortCommand(long cohortId, long accountId, long? changeOfPartyRequestId, UserInfo userInfo, Party lastApprovedBy)
    {
        CohortId = cohortId;
        AccountId = accountId;
        ChangeOfPartyRequestId = changeOfPartyRequestId;
        UserInfo = userInfo;
        LastApprovedBy = lastApprovedBy;
    }
}