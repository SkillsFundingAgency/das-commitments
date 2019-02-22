using System;
using MediatR;

namespace SFA.DAS.ProviderRelationships.Application.Commands.AddAccountLegalEntity
{
    public class AddAccountLegalEntityCommand : IRequest
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public string AccountLegalEntityPublicHashedId { get; }
        public string OrganisationName { get; }
        public DateTime Created { get; }

        public AddAccountLegalEntityCommand(long accountId, long accountLegalEntityId, string accountLegalEntityPublicHashedId, string organisationName, DateTime created)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
            OrganisationName = organisationName;
            Created = created;
        }
    }
}