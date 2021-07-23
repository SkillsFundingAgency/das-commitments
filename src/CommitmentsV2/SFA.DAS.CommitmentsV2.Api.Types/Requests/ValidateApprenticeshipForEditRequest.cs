using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class ValidateApprenticeshipForEditRequest 
    {
        public long ApprenticeshipId { get; set; }
        public long? EmployerAccountId { get; set; }
        public long? ProviderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal? Cost { get; set; }
        public string EmployerReference { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ULN { get; set; }
        public string TrainingCode { get; set; }
        public string ProviderReference { get; set; }
        public string Email { get; set; }
    }
}