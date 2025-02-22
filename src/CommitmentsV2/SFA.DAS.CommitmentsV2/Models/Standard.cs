namespace SFA.DAS.CommitmentsV2.Models;

public class Standard
{
    public string StandardUId { get; set; }
    public int LarsCode { get; set; }
    public string IFateReferenceNumber { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public int Level { get; set; }
    public int Duration { get; set; }
    public int MaxFunding { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public virtual List<StandardFundingPeriod> FundingPeriods { get; set; }
    public virtual List<StandardOption> Options { get; set; }
    public bool IsLatestVersion { get; set; }
    public int VersionMajor { get; set; }
    public int VersionMinor { get; set; }
    public string StandardPageUrl { get; set; }
    public DateTime? VersionEarliestStartDate { get; set; }
    public DateTime? VersionLatestStartDate { get; set; }
    public string Route { get; set; }
}

public class StandardFundingPeriod : IFundingPeriod
{
    public int Id { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int FundingCap { get; set; }
    public virtual Standard Standard { get; set; }
}