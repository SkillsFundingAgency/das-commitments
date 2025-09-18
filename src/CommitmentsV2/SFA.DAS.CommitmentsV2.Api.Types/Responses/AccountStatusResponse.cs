using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class AccountStatusResponse
{
    public List<AccountStatusProviderCourse> Active { get; set; } = new();

    
    public List<AccountStatusProviderCourse> Completed { get; set; } = new();

    
    public List<AccountStatusProviderCourse> NewStart { get; set; } = new();
}

public sealed class AccountStatusProviderCourse
{
    public long Ukprn { get; set; }

    public string CourseCode { get; set; }
}
