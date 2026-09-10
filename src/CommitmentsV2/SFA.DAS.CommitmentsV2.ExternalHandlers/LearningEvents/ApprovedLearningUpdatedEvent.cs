using System;
using System.Collections.Generic;

namespace SFA.DAS.Learning.Types;

public class ApprovedLearningUpdatedEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public string LearningType { get; set; }
    public string LearningUri { get; set; }
    public List<ApprenticeshipFieldChange> Changes { get; set; } = new();
}

public class ApprenticeshipFieldChange
{
    public string ChangeType { get; set; }
    public ApprenticeshipData Data { get; set; }
}

public class ApprenticeshipData
{
    public string Old { get; set; }
    public string New { get; set; }
}