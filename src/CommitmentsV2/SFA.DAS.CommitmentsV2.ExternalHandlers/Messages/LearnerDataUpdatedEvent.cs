namespace SFA.DAS.CommitmentsV2.ExternalHandlers.Messages;

public class LearnerDataUpdatedEvent
{
    public long LearnerId { get; init; }
    public ChangeSummary ChangeSummary { get; init; } = new();
} 