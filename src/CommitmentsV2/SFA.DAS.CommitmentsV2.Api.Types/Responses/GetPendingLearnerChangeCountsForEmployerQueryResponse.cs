namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetPendingLearnerChangeCountsForEmployerQueryResponse
{
    public int ManualPendingChangeCount { get; set; }
    public int IlrPendingChangeCount { get; set; }
}