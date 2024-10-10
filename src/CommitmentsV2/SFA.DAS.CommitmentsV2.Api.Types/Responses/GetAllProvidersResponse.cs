using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetAllProvidersResponse
{
    public List<Provider> Providers { get; set; }
}

public class Provider
{
    public long Ukprn { get; set; }
    public string Name { get; set; }
}