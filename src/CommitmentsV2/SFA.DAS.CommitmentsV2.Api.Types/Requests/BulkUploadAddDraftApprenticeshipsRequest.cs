﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class BulkUploadAddDraftApprenticeshipsRequest : SaveDataRequest
    {
        public IEnumerable<BulkUploadAddDraftApprenticeshipRequest> DraftApprenticeshipRequest { get; set; }
    }

    public class BulkUploadAddDraftApprenticeshipRequest
    {
        public long LegalEntityId { get; set; }
        public long CohortId { get; set; }
        public string UserId { get; set; }
        public long ProviderId { get; set; }
        public string CourseCode { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OriginatorReference { get; set; }
        public Guid? ReservationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Uln { get; set; }
        public string ProviderRef { get; set; }
    }
}