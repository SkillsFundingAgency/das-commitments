using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class DraftApprenticeshipDetails
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Uln { get; set; }
        public TrainingProgramme TrainingProgramme { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
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
    }
}
