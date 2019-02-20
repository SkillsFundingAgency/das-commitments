using SFA.DAS.NServiceBus;
using System;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class ChangedAccountNameEvent : Event
    {
        public long AccountId { get; set; }
        public string HashedAccountId { get; set; }
        public string UserName { get; set; }
        public Guid UserRef { get; set; }
        public string PreviousName { get; set; }
        public string CurrentName { get; set; }
    }
}
