using System;
using MediatR;

namespace SFA.DAS.ProviderRelationships.Application.Commands.RemoveAccountLegalEntity
{
    public class RemoveAccountLegalEntityCommand : IRequest
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public DateTime Removed { get; }

        public RemoveAccountLegalEntityCommand(long accountId, long accountLegalEntityId, DateTime removed)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            Removed = removed;
        }
    }
}