namespace SFA.DAS.CommitmentsV2.Types
{
    public enum OverlappingTrainingDateRequestResolutionType : short
    {
        CompletionDateEvent = 0,
        ApprenticeshipUpdate = 1,
        StopDateUpdate = 2,
        ApprenticeshipStopped = 3,
        DraftApprenticeshipUpdated = 4,
        DraftApprenticeshipDeleted = 5,
        ApprenticeshipIsStillActive = 6,
        ApprenticeshipEndDateUpdate = 7,
        ApprenticeshipStopDateIsCorrect = 8,
        ApprenticeshipEndDateIsCorrect = 9,
    }
}