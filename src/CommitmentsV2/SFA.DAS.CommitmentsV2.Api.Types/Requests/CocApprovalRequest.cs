using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class CocApprovalRequest
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public string LearningType { get; set; }
    public string UKPRN { get; set; }
    public string ULN { get; set; }
    public long AgreementId { get; set; }
    public string ApprovedUri { get; set; }
    public List<CocApprovalFieldChange> Changes { get; set; }
}

public class CocApprovalFieldChange
{
    public string ChangeType { get; set; }
    public CocData Data { get; set; }
}

public class CocData
{
    public string Old { get; set; }
    public string New { get; set; }
}

