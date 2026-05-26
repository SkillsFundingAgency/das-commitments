using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;

public class StopApprenticeshipCommand : IRequest
{
    public UserInfo UserInfo { get; }
    public long AccountId { get; }
    public long ApprenticeshipId { get; }
    public DateTime StopDate { get; }

    public bool MadeRedundant { get; }
    public Party Party { get; set; }
    public StopSource StopSource { get; }
    public int? WithdrawnReasonCode { get; }
    public Guid? LearningKey { get; }
    public DateTime? AppliedDate { get; }

    public StopApprenticeshipCommand(
        long accountId,
        long apprenticeshipId,
        DateTime stopDate,
        bool madeRedundant,
        UserInfo userInfo,
        Party party,
        StopSource stopSource = StopSource.Employer,
        int? withdrawnReasonCode = null,
        Guid? learningKey = null,
        DateTime? appliedDate = null)
    {
        AccountId = accountId;
        ApprenticeshipId = apprenticeshipId;
        StopDate = stopDate;
        MadeRedundant = madeRedundant;
        UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        Party = party;
        StopSource = stopSource;
        WithdrawnReasonCode = withdrawnReasonCode;
        LearningKey = learningKey;
        AppliedDate = appliedDate;
    }
}