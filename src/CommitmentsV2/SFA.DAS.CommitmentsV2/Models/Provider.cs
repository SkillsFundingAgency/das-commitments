using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

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

        public long UkPrn { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public virtual Commitment CreateCohort(AccountLegalEntity accountLegalEntity,
            DraftApprenticeshipDetails draftApprenticeshipDetails,
            IUlnValidator ulnValidator,
            ICurrentDateTime currentDateTime,
            IAcademicYearDateProvider academicYearDateProvider)
        {
            var commitment = new Commitment
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
                Originator = Originator.Provider
            };

            commitment.AddDraftApprenticeship(draftApprenticeshipDetails, ulnValidator, currentDateTime, academicYearDateProvider);  

            return commitment;
        }
    }
}
