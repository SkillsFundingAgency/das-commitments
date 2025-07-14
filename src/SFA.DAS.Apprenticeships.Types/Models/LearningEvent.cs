using System;

namespace SFA.DAS.Learning.Types.Models
{
    public abstract class LearningEvent
    {
        public LearningEpisode Episode { get; set; }
        public Guid LearningKey { get; set; }
    }
}