using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Entities.TrainingProgramme
{
    public class StandardsView
    {
        public DateTime CreationDate { get; set; }
        public List<Standard> Standards { get; set; }
    }
}