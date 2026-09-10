using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class ApprovalRequestUpdateAlertAcknowledge : SaveDataRequest
{
    public long ApprenticeshipId { get; set; }
    public long AccountId { get; set; }
    public List<UpdateApprovalRequestAlertAcknowledgeItem> ApprovalRequestAlerts { get; set; }
}

public class UpdateApprovalRequestAlertAcknowledgeItem
{
    public Guid ApprovalRequestId { get; set; }
    public bool Acknowledged { get; set; }
    public UserInfo UserInfo { get; set; }
}