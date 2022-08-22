using System;
using SFA.DAS.CommitmentsV2.Types;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class OverlappingTrainingDateEvent
    {
        public long ApprenticeshipId { get; }
        public string Uln { get; set; }

        public OverlappingTrainingDateEvent(long apprenticeshipId, string uln)
        {
            ApprenticeshipId = apprenticeshipId;
            Uln = uln;
        }
    }
}
