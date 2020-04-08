using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest
{
    public class ChangeOfPartyRequestCommand : IRequest
    {
        public long ApprenticeshipId { get; set; }
        public ChangeOfPartyRequestType ChangeOfPartyRequestType { get; set; }
        public long NewPartyId { get; set; }
        public int? NewPrice { get; set; }
        public DateTime? NewStartDate { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}