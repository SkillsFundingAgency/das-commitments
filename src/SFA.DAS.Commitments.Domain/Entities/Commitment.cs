﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class Commitment
    {
        public Commitment()
        {
            Apprenticeships = new List<Apprenticeship>();
            Messages = new List<Message>();
        }

        public long Id { get; set; }
        public string Reference { get; set; }
        public long EmployerAccountId { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string LegalEntityAddress { get; set; }
        public SFA.DAS.Common.Domain.Types.OrganisationType LegalEntityOrganisationType { get; set; }
        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public CommitmentStatus CommitmentStatus { get; set; }
        public EditStatus EditStatus { get; set; }
        public LastAction LastAction { get; set; }
        public bool EmployerCanApproveCommitment { get; set; }
        public bool ProviderCanApproveCommitment { get; set; }

        public string LastUpdatedByEmployerName { get; set; }
        public string LastUpdatedByEmployerEmail { get; set; }
        public string LastUpdatedByProviderName { get; set; }
        public string LastUpdatedByProviderEmail { get; set; }

        [JsonIgnore]
        public List<Apprenticeship> Apprenticeships { get; set; }
        [JsonIgnore]
        public List<Message> Messages { get; set; }
    }
}
