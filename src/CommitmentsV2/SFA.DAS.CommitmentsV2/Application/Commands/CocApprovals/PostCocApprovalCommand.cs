using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;

public class PostCocApprovalCommand() : IRequest<CocApprovalResult>
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public Apprenticeship Apprenticeship { get; set; }
    public CocLearningType LearningType { get; set; }
    public long ProviderId { get; set; }
    public string ULN { get; set; }
    public CocUpdates Updates { get; set; }
    public List<CocApprovalFieldChange> ApprovalFieldChanges { get; set; }
}

public class CocUpdates
{
    public CocUpdate<int> TNP1 { get; set; }
    public CocUpdate<int> TNP2 { get; set; }
}

public class CocUpdate<T> where T : struct
{
    public T? New { get; set; }
    public T? Old { get; set; }
}