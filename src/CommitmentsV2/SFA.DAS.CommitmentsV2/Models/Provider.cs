using System;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Provider
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

        public virtual Cohort CreateCohort(AccountLegalEntity accountLegalEntity, DraftApprenticeshipDetails draftApprenticeshipDetails, Originator party)
        {
            var cohort = new Cohort
            {
                // Reference cannot be set until we've saved the commitment (as we need the Id) but it's non-nullable so we'll use a temp value
                Reference = "",
                EmployerAccountId = accountLegalEntity.AccountId,
                LegalEntityId = accountLegalEntity.LegalEntityId,
                LegalEntityName = accountLegalEntity.Name,
                LegalEntityAddress = accountLegalEntity.Address,
                LegalEntityOrganisationType = accountLegalEntity.OrganisationType,
                ProviderId = UkPrn,
                ProviderName = Name,
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = EditStatus.ProviderOnly,
                CreatedOn = DateTime.UtcNow,
                LastAction = LastAction.None,
                AccountLegalEntityPublicHashedId = accountLegalEntity.PublicHashedId,
                Originator = party
            };

            cohort.AddDraftApprenticeship(draftApprenticeshipDetails, party); 

            return cohort;
        }
    }
}
