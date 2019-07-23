using System;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Provider : ICohortOriginator
    {
        public Provider()
        {
        }

        internal Provider(long ukPrn, string name, DateTime created, DateTime updated)
        {
            UkPrn = ukPrn;
            Name = name;
            Created = created;
            Updated = updated;
        }

        public virtual long UkPrn { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public virtual Cohort CreateCohort(Provider provider,
            AccountLegalEntity accountLegalEntity,
            DraftApprenticeshipDetails draftApprenticeshipDetails,
            Party initialParty,
            string message,
            UserInfo userInfo)
        {
            return new Cohort(this, accountLegalEntity, draftApprenticeshipDetails, initialParty, Party.Provider, message, userInfo);
        }
    }
}
