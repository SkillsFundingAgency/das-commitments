using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class CreateChangeOfPartyRequestRequest : SaveDataRequest
    {
        public ChangeOfPartyRequestType ChangeOfPartyRequestType { get; set; }
        public long NewPartyId { get; set; }
        public int? NewPrice { get; set; }
        public DateTime? NewStartDate { get; set; }
        public DateTime? NewEndDate { get; set; }
        public DateTime? NewEmploymentEndDate { get; set; }
        public int? NewEmploymentPrice { get; set; }
        public DeliveryModel? DeliveryModel { get; set; }
    }
}