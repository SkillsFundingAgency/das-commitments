﻿using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetApprenticeshipResponse
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
    public string Email { get; set; }
    public string Uln { get; set; }
    public string NINumber { get; set; }
    public string CourseCode { get; set; }
    public string StandardUId { get; set; }
    public string Version { get; set; }
    public string Option { get; set; }
    public string CourseName { get; set; }
    public DeliveryModel DeliveryModel { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime DateOfBirth { get; set; }        
    public string EmployerReference { get; set; }
    public string ProviderReference { get; set; }
    public ApprenticeshipStatus Status { get; set; }
    public DateTime? StopDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string EndpointAssessorName { get; set; }
    public bool HasHadDataLockSuccess { get; set; }
    public long? ContinuationOfId { get; set; }
    public long? ContinuedById { get; set; }
    public DateTime? OriginalStartDate { get; set; }
    public bool IsContinuation => ContinuationOfId.HasValue;
    public bool HasContinuation => ContinuedById.HasValue;
    public long? PreviousProviderId { get; set; }
    public long? PreviousEmployerAccountId { get; set; }
    public ApprenticeshipEmployerType? ApprenticeshipEmployerTypeOnApproval { get; set; }
    public bool? MadeRedundant { get; set; }
    public ConfirmationStatus? ConfirmationStatus { get; set; }
    public bool EmailAddressConfirmedByApprentice { get; set; }
    public bool EmailShouldBePresent { get; set; }
    public int? PledgeApplicationId { get; set; }
    public int? EmploymentPrice { get; set; }
    public DateTime? EmploymentEndDate { get; set; }
    public bool? RecognisePriorLearning { get; set; }
    public int? DurationReducedBy { get; set; }
    public int? PriceReducedBy { get; set; }
    public long? TransferSenderId { get; set; }
    public bool? IsOnFlexiPaymentPilot { get; set; }
    public int? DurationReducedByHours { get; set; }
    public int? TrainingTotalHours { get; set; }
    public bool? IsDurationReducedByRpl { get; set; }
}