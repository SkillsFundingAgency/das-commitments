using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class ProviderStandardResults
{
    public bool IsMainProvider { get; set; }
    public IEnumerable<ProviderStandard> Standards { get; set; } = new List<ProviderStandard>();
}

public class ProviderStandard
{
    public ProviderStandard(string courseCode, string name)
    {
        CourseCode = courseCode;
        Name = name;
    }

    public string CourseCode { get; }
    public string Name { get; }
}