using System;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class UserJoinedEvent : Event
    {
        public long AccountId { get; set; }
        public string UserName { get; set; }
        public Guid UserRef { get; set; }
        public UserRole Role { get; set; }
    }
}
