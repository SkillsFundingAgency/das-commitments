using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain
{
    public class Commitment
    {
        public Commitment()
        {
            Apprenticeships = new List<Apprenticeship>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public long EmployerAccountId { get; set; }
        public long LegalEntityId { get; set; }
        public long? ProviderId { get; set; }

        public List<Apprenticeship> Apprenticeships { get; set; }
    }
}
