using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class CreateCohortRequest : SaveDataRequest
    {
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long ProviderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Uln { get; set; }
        public string CourseCode { get; set; }
        public DeliveryModel? DeliveryModel { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OriginatorReference { get; set; }
        public Guid? ReservationId { get; set; }
        public long? TransferSenderId { get; set; }
        public int? PledgeApplicationId { get; set; }
    }
}