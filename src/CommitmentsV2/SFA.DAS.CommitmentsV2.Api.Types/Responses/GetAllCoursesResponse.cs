using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetAllCoursesResponse
{
    public List<Course> Courses { get; set; }
}

public class Course
{
    public string LarsCode { get; set; }
    public string Title { get; set; }
    public string Level { get; set; }
    public string LearningType { get; set; }
    public int MaxFunding { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}