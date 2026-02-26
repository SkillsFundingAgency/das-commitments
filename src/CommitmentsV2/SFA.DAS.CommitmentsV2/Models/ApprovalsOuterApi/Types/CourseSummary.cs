using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

public class CourseSummary
{
    public string LarsCode { get; set; }
    public string Title { get; set; }
    public int Level { get; set; }
    public LearningType LearningType { get; set; }
    public int MaxFunding { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public byte LearningTypeByte => (byte)LearningType;
}

public class CourseResponse
{
    public IEnumerable<CourseSummary> Courses { get; set; }
}