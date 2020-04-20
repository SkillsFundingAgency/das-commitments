﻿using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetApprenticeshipsResponse
    {
        public IEnumerable<ApprenticeshipDetailsResponse> Apprenticeships { get; set; }
        public int TotalApprenticeshipsFound { get; set; }
        public int TotalApprenticeshipsWithAlertsFound { get; set; }
        public int TotalApprenticeships { get; set; }
        public int PageNumber { get; set; }

        public class ApprenticeshipDetailsResponse
        {
            public long Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Uln { get; set; }
            public string EmployerName { get; set; }
            public string ProviderName { get; set; }
            public string CourseName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime PauseDate { get; set; }
            public DateTime DateOfBirth { get; set; }
            public PaymentStatus PaymentStatus { get; set; }
            public ApprenticeshipStatus ApprenticeshipStatus { get; set; }
            public IEnumerable<Alerts> Alerts { get; set; }
            public decimal? TotalAgreedPrice { get; set; }
            public string EmployerRef { get; set; }
            public string ProviderRef { get; set; }
            public string CohortReference { get; set; }
            public long AccountLegalEntityId { get; set; }
        }
    }
}
