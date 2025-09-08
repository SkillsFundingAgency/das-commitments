﻿using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;

public class UpdateDraftApprenticeshipCommand : IRequest<UpdateDraftApprenticeshipResponse>
{
    public Party? RequestingParty { get; set; }
    public long CohortId { get; set; }
    public long ApprenticeshipId { get; set; }
    public string CourseCode { get; set; }
    public string CourseOption { get; set; }
    public DeliveryModel DeliveryModel { get; set; }
    public int? EmploymentPrice { get; set; }
    public int? Cost { get; set; }
    public int? TrainingPrice { get; set; }
    public int? EndPointAssessmentPrice { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? EmploymentEndDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reference { get; set; }
    public Guid? ReservationId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Uln { get; set; }
    public UserInfo UserInfo { get; set; }
    public bool IgnoreStartDateOverlap { get; set; }
    public bool? IsOnFlexiPaymentPilot { get; set; }
    public bool IsContinuation { get; set; }
    public int MinimumAgeAtApprenticeshipStart { get; set; }
    public int MaximumAgeAtApprenticeshipStart { get; set; }
    public bool HasLearnerDataChanges { get; set; }
    public DateTime? LastLearnerDataSync { get; set; }
}