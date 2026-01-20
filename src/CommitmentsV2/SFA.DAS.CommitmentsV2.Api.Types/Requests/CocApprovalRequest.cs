using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class CocApprovalRequest
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public CocLearningType LearningType { get; set; }
    public string UKPRN { get; set; }
    public string ULN { get; set; }
    public long AgreementId { get; set; }
    public string ApprovedUri { get; set; }
    public List<CocApprovalFieldChange> Changes { get; set; }
}

public class CocApprovalFieldChange
{
    public CocChangeField ChangeType { get; set; }
    public CocData Data { get; set; }
}

public class CocData
{
    public string Old { get; set; }
    public string New { get; set; }
}

public enum CocLearningType
{
    Apprenticeship,
    FoundationApprenticeship,
    ApprenticeshipUnit
}

public enum CocChangeField
{
    TNP1,
    TNP2
}