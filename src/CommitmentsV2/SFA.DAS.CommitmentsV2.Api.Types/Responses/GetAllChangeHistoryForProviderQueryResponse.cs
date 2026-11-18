using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetAllChangeHistoryForProviderQueryResponse
{
    public List<ChangeHistory> ChangeHistory { get; set; }
    public int TotalCount { get; set; }
    public DateTime? AvailableFrom { get; set; }
}
