using System;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;

namespace SFA.DAS.Commitments.Api.Types.Apprenticeship
{
    public sealed class ApprenticeshipStopDate
    {
        public DateTime NewStopDate { get; set; }
        public string UserId { get; set; }
        public LastUpdateInfo LastUpdatedByInfo { get; set; }
    }
}