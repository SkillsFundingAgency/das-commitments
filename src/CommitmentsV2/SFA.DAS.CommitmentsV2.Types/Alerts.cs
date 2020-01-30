using System;

namespace SFA.DAS.CommitmentsV2.Types
{
    [Flags]
    public enum Alerts
    {
        IlrDataMismatch = 0,
        ChangesPending = 1,
        ChangesRequested = 2,
        ChangesForReview = 3
    }
}