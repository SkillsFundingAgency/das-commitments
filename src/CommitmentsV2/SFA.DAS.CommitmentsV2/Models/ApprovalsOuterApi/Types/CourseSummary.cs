using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

public class CourseSummary
{
    public string LarsCode { get; set; }
    public string Title { get; set; }
    public int Level { get; set; }
    public string LearningType { get; set; }
    [JsonProperty("maxFunding")]
    public int CurrentFundingCap { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class CourseResponse
{
    public IEnumerable<CourseSummary> Courses { get; set; }
}