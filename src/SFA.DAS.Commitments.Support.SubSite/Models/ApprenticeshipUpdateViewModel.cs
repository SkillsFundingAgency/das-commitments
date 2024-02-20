using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class ApprenticeshipUpdateViewModel
    {
        public string OriginalFirstName { get; set; }
        public string OriginalLastName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ULN { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Version { get; set; }
        public string Option { get; set; }
        public DeliveryModel? DeliveryModel { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public int? EmploymentPrice { get; set; }
        public decimal? Cost { get; set; }
        public Originator Originator { get; set; }
        public DateTime? CreatedOn { get; set; }

        public string DisplayNameForUpdate
        {
            get
            {
                return (string.IsNullOrWhiteSpace(FirstName) ? OriginalFirstName : FirstName) + " " + (string.IsNullOrWhiteSpace(LastName) ? OriginalLastName : LastName);
            }
        }
    }
}
