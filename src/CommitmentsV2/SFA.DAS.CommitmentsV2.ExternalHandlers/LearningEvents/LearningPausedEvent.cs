using System;

namespace SFA.DAS.Learning.Types;

// Replace with SFA.DAS.Learning.Types NuGet package when LearningPausedEvent is published.
public class LearningPausedEvent
{
    public long ApprenticeshipId { get; set; }

    public DateTime PauseDate { get; set; }

    public Guid LearningKey { get; set; }

    public DateTime Created { get; set; }
}
