using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortWithOtherPartyCommand : IRequest<AddCohortResult>
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public long ProviderId { get; }
        public long? TransferSenderId { get; }
        public int? PledgeApplicationId { get; }
        public string Message { get; }
        public UserInfo UserInfo { get; }

        public AddCohortWithOtherPartyCommand(long accountId, long accountLegalEntityId, long providerId, long? transferSenderId, int? pledgeApplicationId, string message, UserInfo userInfo)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            ProviderId = providerId;
            TransferSenderId = transferSenderId;
            PledgeApplicationId = pledgeApplicationId;
            Message = message;
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        }
    }
}
