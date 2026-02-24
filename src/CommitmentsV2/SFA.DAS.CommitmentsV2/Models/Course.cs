using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public class Course
{
    public string LarsCode { get; set; }
    public string Title { get; set; }
    public string Level { get; set; }
    public LearningType? LearningType { get; set; }
    public int MaxFunding { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}