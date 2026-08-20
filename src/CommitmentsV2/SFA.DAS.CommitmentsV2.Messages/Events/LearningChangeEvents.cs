using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Messages.Events;


public class LearningChangeApprovedEvent : LearningChangeEvent { }

public class LearningChangeRejectedEvent : LearningChangeEvent { }


public class LearningChangeEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public Dictionary<string, Change> Changes { get; set; }

    public class Change
    {
        public string Old { get; set; }
        public string New { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
    }
}