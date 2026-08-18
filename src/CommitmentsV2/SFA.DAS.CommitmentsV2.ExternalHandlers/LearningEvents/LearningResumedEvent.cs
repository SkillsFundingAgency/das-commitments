using System;

namespace SFA.DAS.Learning.Types;

// Replace with SFA.DAS.Learning.Types NuGet package when LearningResumedEvent is published.

public class LearningResumedEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public DateTime Created { get; set; }
    public DateTime ResumeDate { get; set; }
}