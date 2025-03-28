using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

public class StandardSummary
{
    public string StandardUId { get; set; }
    public int LarsCode { get; set; }
    public string IFateReferenceNumber { get; set; }
    public string Version { get; set; }
    public int Level { get; set; }
    public string Title { get; set; }
    public int Duration { get; set; }
    [JsonProperty("maxFunding")]
    public int CurrentFundingCap { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? LastDateForNewStarts { get; set; }
    [JsonProperty("apprenticeshipFunding")]
    public IEnumerable<FundingPeriodItem> FundingPeriods { get; set; }
    public VersionDetail VersionDetail { get; set; }
    public int VersionMajor { get; set; }
    public int VersionMinor { get; set; }
    public string StandardPageUrl { get; set; }
    public string Status { get; set; }
    public bool IsLatestVersion { get; set; }
    public string[] Options { get; set; }
    public string Route { get; set; }
    public string ApprenticeshipType { get; set; }

    /// <summary>
    /// Property to allow the ToDataTable Extension to function with the Lambda Expression
    /// </summary>
    public DateTime? VersionEarliestStartDate
    {
        get
        {
            return VersionDetail.EarliestStartDate ?? null;
        }
    }

    /// <summary>
    /// Property to allow the ToDataTable Extension to function with the Lambda Expression
    /// </summary>
    public DateTime? VersionLatestStartDate
    {
        get
        {
            return VersionDetail.LatestStartDate ?? null;
        }
    }
}

public class VersionDetail
{
    public DateTime? EarliestStartDate { get; set; }
    public DateTime? LatestStartDate { get; set; }
    public DateTime? LatestEndDate { get; set; }
    public DateTime? ApprovedForDelivery { get; set; }
}

public class StandardResponse
{
    public IEnumerable<StandardSummary> Standards { get; set; }
}