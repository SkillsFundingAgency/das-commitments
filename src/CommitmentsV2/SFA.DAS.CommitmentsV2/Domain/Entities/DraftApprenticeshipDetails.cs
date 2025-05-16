using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class DraftApprenticeshipDetails
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Uln { get; set; }
    public TrainingProgramme TrainingProgramme { get; set; }
    public string TrainingCourseVersion { get; set; }
    public bool TrainingCourseVersionConfirmed { get; set; }
    public string TrainingCourseOption { get; set; }
    public DeliveryModel? DeliveryModel { get; set; }
    public int? Cost { get; set; }
    public int? TrainingPrice { get; set; }
    public int? EndPointAssessmentPrice { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? EmploymentPrice { get; set; }
    public DateTime? EmploymentEndDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Reference { get; set; }
    public Guid? ReservationId { get; set; }
    public int? AgeOnStartDate
    {
        get
        {
            if (StartDate.HasValue && DateOfBirth.HasValue)
            {
                var age = StartDate.Value.Year - DateOfBirth.Value.Year;

                if ((DateOfBirth.Value.Month > StartDate.Value.Month) ||
                    (DateOfBirth.Value.Month == StartDate.Value.Month &&
                     DateOfBirth.Value.Day > StartDate.Value.Day))
                    age--;

                return age;
            }

            return default;
        }
    }

    public string StandardUId { get ; set ; }
    public bool? RecognisePriorLearning { get; set; }

    public int? DurationReducedBy { get; set; }

    public int? PriceReducedBy { get; set; }

    public bool IgnoreStartDateOverlap { get; set; }
    public bool? IsOnFlexiPaymentPilot { get; set; }
    public int? DurationReducedByHours { get; set; }
    public int? TrainingTotalHours { get; set; }
    public bool? IsDurationReducedByRPL { get; set; }
    public bool? EmployerHasEditedCost { get; set; }
    public long? LearnerDataId { get; set; }
}