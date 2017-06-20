﻿using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class Apprenticeship
    {
        public Apprenticeship()
        {
            PriceHistory = new List<PriceHistory>();
        }

        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public string Reference { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NINumber { get; set; }
        public string ULN { get; set; }
        public TrainingType TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public AgreementStatus AgreementStatus { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public bool EmployerCanApproveApprenticeship { get; set; }
        public bool ProviderCanApproveApprenticeship { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? AgreedOn { get; set; }
        public int PaymentOrder { get; set; }
        public Originator? UpdateOriginator { get; set; }
        public string ProviderName { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public bool DataLockPrice { get; set; }
        public bool DataLockPriceTriaged { get; set; }
        public bool DataLockCourse { get; set; }
        public bool DataLockCourseTriaged { get; set; }
        public List<PriceHistory> PriceHistory { get; set; }

        public Apprenticeship Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Apprenticeship>(json);
        }
    }
}
