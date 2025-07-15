using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class EditApprenticeshipApiRequest : SaveDataRequest
{
    public long? ProviderId { get; set; }
    public long? AccountId { get; set; }
    public long ApprenticeshipId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string ULN { get; set; }
    public DeliveryModel? DeliveryModel { get; set; }
    public DateTime? EmploymentEndDate { get; set; }
    public int? EmploymentPrice { get; set; }
    public string TrainingName { get; set; }
    public decimal? Cost { get; set; }
    public decimal? TrainingPrice { get; set; }
    public decimal? EndPointAssessmentPrice { get; set; }
    public string EmployerReference { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string CourseCode { get; set; }
    public string Version { get; set; }
    public string Option { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ProviderReference { get; set; }
    public int MinimumAgeAtApprenticeshipStart { get; set; }
    public int MaximumAgeAtApprenticeshipStart { get; set; }
}