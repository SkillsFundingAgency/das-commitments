using System;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class CreatedAgreementEvent : Event
    {
        public long AccountId { get; set; }
        public string UserName { get; set; }
        public Guid UserRef { get; set; }
        public string OrganisationName { get; set; }
        public long AgreementId { get; set; }
        public long LegalEntityId { get; set; }
    }
}
