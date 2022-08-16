namespace SFA.DAS.CommitmentsV2.Types
{
    public enum OverlappingTrainingDateRequestResolutionType : short
    {
        CompletionDateEvent = 0,
        ApprenticeshipUpdate = 1,
        StopDateUpdate = 2,
        ApprenticeshipStopped = 3,
        DraftApprenticeshipUpdated = 4,
        DraftApprentieshipDeleted = 5,
        ApprentieshipIsStillActive = 6
    }
}