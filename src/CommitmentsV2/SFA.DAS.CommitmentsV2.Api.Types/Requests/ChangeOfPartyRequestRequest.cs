using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class ChangeOfPartyRequestRequest : SaveDataRequest
    {
        public ChangeOfPartyRequestType ChangeOfPartyRequestType { get; set; }
        public long NewPartyId { get; set; }
        public int? NewPrice { get; set; }
        public DateTime? NewStartDate { get; set; }
    }
}