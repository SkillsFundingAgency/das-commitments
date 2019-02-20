using System;
using System.Collections.Generic;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class AccountUserRolesUpdatedEvent : Event
    {
        public long AccountId { get; }
        public Guid UserRef { get; }
        public UserRole Role { get; }

        public AccountUserRolesUpdatedEvent(long accountId, Guid userRef, UserRole role, DateTime created)
        {
            AccountId = accountId;
            UserRef = userRef;
            Role = role;
            Created = created;
        }
    }
}
