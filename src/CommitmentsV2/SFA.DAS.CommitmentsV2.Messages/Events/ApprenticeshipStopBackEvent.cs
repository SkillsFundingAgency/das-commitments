using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipStopBackEvent
    {
        public long? ApprenticeshipId { get; set; }
        public string Uln { get; set; }
        public long ProviderId { get; set; }
        public long? LearnerDataId { get; set; }
    }
}
