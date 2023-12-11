using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest
{
    public class CreateChangeOfPartyRequestCommand : IRequest
    {
        public long ApprenticeshipId { get; set; }
        public ChangeOfPartyRequestType ChangeOfPartyRequestType { get; set; }
        public long NewPartyId { get; set; }
        public int? NewPrice { get; set; }
        public DateTime? NewStartDate { get; set; }
        public DateTime? NewEndDate { get; set; }
        public UserInfo UserInfo { get; set; }
        public int? NewEmploymentPrice { get; set; }
        public DateTime? NewEmploymentEndDate { get; set; }
        public DeliveryModel? DeliveryModel { get; set; }
        public bool HasOLTD { get; set; }
    }
}