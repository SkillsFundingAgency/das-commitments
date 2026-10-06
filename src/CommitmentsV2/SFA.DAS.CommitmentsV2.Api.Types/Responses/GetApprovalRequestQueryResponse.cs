using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetApprovalRequestQueryResponse
{
    public string ApprenticeName { get; set; }
    public List<ApprovalRequestItem> ApprovalRequests { get; set; }
}

public class ApprovalRequestItem
{
    public Guid Id { get; set; }
    public long ApprenticeshipId { get; set; }
    public byte? Status { get; set; }
    public virtual ICollection<ApprovalFieldRequest> Items { get; set; }
    public DateTime Created { get; set; }
}

public class ApprovalFieldRequest
{
    public string Field { get; set; }
    public string Old { get; set; }
    public string New { get; set; }
    public byte? Status { get; set; }
    public DateTime Created { get; set; }
}