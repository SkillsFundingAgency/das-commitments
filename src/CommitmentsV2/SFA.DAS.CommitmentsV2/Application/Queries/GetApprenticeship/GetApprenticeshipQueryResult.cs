﻿using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship
{
    public class GetApprenticeshipQueryResult
    {
        public long Id { get; set; }
        public long CohortId { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public long EmployerAccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string EmployerName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Uln { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Reference { get; set; }
        public ApprenticeshipStatus Status { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string EndpointAssessorName { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public long? ContinuationOfId { get; set; }
        public long? ContinuedById { get; set; }
        public DateTime? OriginalStartDate { get; set; }
        public long? PreviousEmployerAccountId { get; set; }
        public long? PreviousProviderId { get; set; }
        public ApprenticeshipEmployerType? ApprenticeshipEmployerTypeOnApproval { get; set; }
    }
}
