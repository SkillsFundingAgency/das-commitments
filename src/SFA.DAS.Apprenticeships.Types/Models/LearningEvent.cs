using System;

namespace SFA.DAS.Learning.Types.Models
{
    public abstract class LearningEvent
    {
        public ApprenticeshipEpisode Episode { get; set; }
        public Guid LearningKey { get; set; }
    }
}