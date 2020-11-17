using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ProviderRejectedChangeOfProviderRequestEvent
    {
        public long EmployerAccountId { get; set; }
        public string TrainingProviderName { get; set; }
        public string EmployerName { get; set; }
        public string ApprenticeName { get; set; }
        public string ApprenticeRecordUrl { get; set; }
        public long ChangeOfPartyRequestId { get; set; }
    }
}
