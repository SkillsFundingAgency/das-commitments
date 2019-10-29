using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Shared.Models;

namespace SFA.DAS.CommitmentsV2.Shared.Interfaces
{
    public interface IEmployerAgreementService
    {
        Task<bool> IsAgreementSigned(long accountId, long accountLegalEntityId, params AgreementFeature[] requiredFeatures);
    }
}
