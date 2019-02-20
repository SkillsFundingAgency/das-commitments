using System;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class RemovedLegalEntityEvent : Event
    {
        public long AccountId { get; set; }
        public string UserName { get; set; }
        public Guid UserRef { get; set; }
        public long AgreementId { get; set; }
        public bool AgreementSigned { get; set; }
        public long LegalEntityId { get; set; }
        public string OrganisationName { get; set; }
        public long AccountLegalEntityId { get; set; }
    }
}
