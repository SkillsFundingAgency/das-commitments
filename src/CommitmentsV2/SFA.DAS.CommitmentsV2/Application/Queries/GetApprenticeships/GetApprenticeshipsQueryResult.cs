﻿using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;

public class GetApprenticeshipsQueryResult
{
    public IEnumerable<ApprenticeshipDetails> Apprenticeships { get; set; }
    public int TotalApprenticeshipsFound { get; set; }
    public int TotalApprenticeshipsWithAlertsFound { get; set; }
    public int TotalApprenticeships { get; set; }
    public int PageNumber { get; set; }

    public class ApprenticeshipDetails
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Uln { get; set; }
        public string EmployerName { get; set; }
        public string ProviderName { get; set; }
        public long ProviderId { get; set; }
        public string CourseName { get; set; }
        public DeliveryModel DeliveryModel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime PauseDate { get; set; }
        public DateTime? StopDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public ApprenticeshipStatus ApprenticeshipStatus { get; set; }
        public IEnumerable<Alerts> Alerts { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ProviderRef { get; set; }
        public string EmployerRef { get; set; }
        public decimal? TotalAgreedPrice { get; set; }
        public string CohortReference { get; set; }
        public long AccountLegalEntityId { get; set; }
        public ConfirmationStatus? ConfirmationStatus { get; set; }
        public long? TransferSenderId { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public string CourseCode { get; set; }
        public decimal? Cost { get; set; }
        public int? PledgeApplicationId { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public bool? EmployerHasEditedCost { get; set; }
        public string TrainingCourseVersion { get; set; }
    }
}