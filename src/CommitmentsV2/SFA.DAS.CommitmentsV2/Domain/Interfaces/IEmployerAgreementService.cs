using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IEmployerAgreementService
{
    Task<bool> IsAgreementSigned(long accountId, long maLegalEntityId, params AgreementFeature[] requiredFeatures);
    Task<long?> GetLatestAgreementId(long accountId, long maLegalEntityId);
}