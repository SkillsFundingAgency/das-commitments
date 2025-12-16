using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class ProviderStandardResults
{
    public bool IsMainProvider { get; set; }
    public IEnumerable<ProviderStandard> Standards { get; set; } = new List<ProviderStandard>();
}

public class ProviderStandard
{
    public ProviderStandard(string courseCode, string name, int? level = null)
    {
        CourseCode = courseCode;
        Name = name;
        Level = level;
    }

    public string CourseCode { get; }
    public string Name { get; }
    public int? Level { get; }
}