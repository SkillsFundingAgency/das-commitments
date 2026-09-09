namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingLearnerChangeCount;

public class GetPendingLearnerChangeCountsForEmployerQueryResult
{
    public int ManualPendingChangeCount { get; set; }
    public int IlrPendingChangeCount { get; set; }
}