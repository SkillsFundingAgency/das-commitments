namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

public class GetEmploymentChecksRequest(IReadOnlyList<long> apprenticeshipIds) : IGetApiRequest
{
    private IReadOnlyList<long> ApprenticeshipIds { get; } = apprenticeshipIds;

    public string GetUrl => "EmploymentChecks?" + string.Join("&", ApprenticeshipIds.Select(id => $"apprenticeshipIds={id}"));
}
