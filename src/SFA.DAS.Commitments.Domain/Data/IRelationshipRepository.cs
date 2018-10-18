using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IRelationshipRepository
    {
        Task<Relationship> GetRelationship(long employerAccountId, long providerId, string legalEntityCode);
        Task VerifyRelationship(long employerAccountId, long providerId, string legalEntityCode, bool verified);
    }
}
