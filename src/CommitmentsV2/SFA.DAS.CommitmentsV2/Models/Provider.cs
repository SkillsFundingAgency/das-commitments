﻿using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Provider : ICohortOriginator
    {
        public Provider()
        {
        }

        public Provider(long ukPrn, string name, DateTime created, DateTime updated)
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
        public virtual ICollection<Cohort> Cohorts { get; set; }

        public virtual Cohort CreateCohort(long providerId,
            AccountLegalEntity accountLegalEntity,
            Account transferSender,
            DraftApprenticeshipDetails draftApprenticeshipDetails,
            UserInfo userInfo)
        {
            return new Cohort(providerId, accountLegalEntity.AccountId, accountLegalEntity.Id, transferSender?.Id, draftApprenticeshipDetails, Party.Provider, userInfo);
        }

        public virtual Cohort CreateCohort(long providerId, AccountLegalEntity accountLegalEntity, UserInfo userInfo)
        {
            return new Cohort(providerId, accountLegalEntity.AccountId, accountLegalEntity.Id, Party.Provider, userInfo);
        }
    }
}
