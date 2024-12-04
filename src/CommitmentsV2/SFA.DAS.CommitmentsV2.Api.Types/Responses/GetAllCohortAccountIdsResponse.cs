using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public sealed class GetAllCohortAccountIdsResponse
{
    public List<long> AccountIds { get; }

    public GetAllCohortAccountIdsResponse(List<long> accountIds)
    {
        AccountIds = accountIds;
    }
       
}