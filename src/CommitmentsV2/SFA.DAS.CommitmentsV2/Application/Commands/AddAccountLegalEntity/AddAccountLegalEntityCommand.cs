using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity
{
    public class AddAccountLegalEntityCommand : IRequest
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public string AccountLegalEntityPublicHashedId { get; }
        public string OrganisationName { get; }
        public OrganisationType OrganisationType { get; }
        public string OrganisationReferenceNumber { get; }
        public string OrganisationAddress { get; }
        public DateTime Created { get; }

        public AddAccountLegalEntityCommand(long accountId, long accountLegalEntityId,
            string accountLegalEntityPublicHashedId, string organisationName, OrganisationType organisationType,
            string organisationReferenceNumber, string organisationAddress, DateTime created)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
            OrganisationName = organisationName;
            OrganisationType = organisationType;
            OrganisationReferenceNumber = organisationReferenceNumber;
            OrganisationAddress = organisationAddress;
            Created = created;
        }
    }
}