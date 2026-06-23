using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetChangeHistoryResponse
{
    public List<ChangeHistory> ChangeHistory { get; set; }
}

public class ChangeHistory
{
    public Guid Id { get; set; }
    public byte ChangeType { get; set; }
    public string Description { get; set; }
    public long ApprenticeshipId { get; set; }
    public string LearnerName { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime Created { get; set; }
}