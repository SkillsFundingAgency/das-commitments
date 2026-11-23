namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipLockedFields;

public class GetApprenticeshipLockedFieldsQueryResult
{
    public bool? OnServicePayments { get; set; }
    public List<string> LockedManualFields { get; set; }
}
