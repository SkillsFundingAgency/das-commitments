using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetChangeHistoryForEmployerResponse
{
    public List<ChangeHistory> ChangeHistory { get; set; }
}