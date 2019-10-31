using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Shared.Models;

namespace SFA.DAS.CommitmentsV2.Shared.Interfaces
{
    public interface IEmployerAgreementService
    {
        Task<bool> IsAgreementSigned(long accountId, long maLegalEntityId, CancellationToken cancellationToken = default(CancellationToken), params AgreementFeature[] requiredFeatures);
        Task<long?> GetLatestAgreementId(long accountId, long accountLegalEntityId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
