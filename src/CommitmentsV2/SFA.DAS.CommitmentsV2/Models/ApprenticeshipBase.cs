﻿using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public abstract class ApprenticeshipBase
    {
        public bool IsApproved { get; set; }

        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Uln { get; set; }
        public ProgrammeType? ProgrammeType { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AgreementStatus AgreementStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NiNumber { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? AgreedOn { get; set; }
        public string EpaOrgId { get; set; }
        public long? CloneOf { get; set; }

        public Guid? ReservationId { get; set; }

        public virtual Cohort Cohort { get; set; }
        public virtual AssessmentOrganisation EpaOrg { get; set; }
        
        public virtual ICollection<ApprenticeshipUpdate> ApprenticeshipUpdate { get; set; }
    }
}
