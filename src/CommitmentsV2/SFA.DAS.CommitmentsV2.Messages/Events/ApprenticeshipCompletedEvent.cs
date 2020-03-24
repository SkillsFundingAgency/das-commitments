using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipCompletedEvent
    {
        public long  ApprenticeshipId { get; set; }
        public DateTime CompletionDate { get; set; }
    }
}
