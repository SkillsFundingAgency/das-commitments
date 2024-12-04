using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Services;

public class EmployerAgreementService : IEmployerAgreementService
{
    private readonly IAccountApiClient _accountApiClient;
    private readonly IEncodingService _encodingService;
    private readonly ILogger<EmployerAgreementService> _logger;

    private readonly Dictionary<AgreementFeature, int> _agreementUnlocks;

    public EmployerAgreementService(IAccountApiClient accountApiClient, IEncodingService encodingService, ILogger<EmployerAgreementService> logger)
    {
        _accountApiClient = accountApiClient;
        _encodingService = encodingService;
        _logger = logger;

        //This dictionary indicates which features are unlocked by agreement versions
        _agreementUnlocks = new Dictionary<AgreementFeature, int> { { AgreementFeature.Transfers, 2 } };
    }

    public async Task<bool> IsAgreementSigned(long accountId, long maLegalEntityId, params AgreementFeature[] requiredFeatures)
    {
        bool AreAllRequiredFeaturesPresentInSignedAgreement(int signedAgreement)
        {
            if (requiredFeatures.Any(f => signedAgreement < _agreementUnlocks[f]))
            {
                return false;
            }

            return true;
        }

        try
        {
            var legalEntity = await GetLegalEntity(accountId, maLegalEntityId);

            var signedAgreements = legalEntity.Agreements
                .Where(x => x.Status == EmployerAgreementStatus.Signed).ToList();

            if (signedAgreements.Count == 0)
            {
                return false;
            }

            if (requiredFeatures?.Length > 0)
            {
                return AreAllRequiredFeaturesPresentInSignedAgreement(signedAgreements.Max(x => x.TemplateVersionNumber));
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in EmployerAgreementService.IsAgreementSigned");
            throw;
        }
    }

    public async Task<long?> GetLatestAgreementId(long accountId, long maLegalEntityId)
    {
        try
        {
            var legalEntity = await GetLegalEntity(accountId, maLegalEntityId);

            var latestAgreement = legalEntity.Agreements.OrderByDescending(x => x.TemplateVersionNumber).FirstOrDefault();

            return latestAgreement?.Id;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in EmployerAgreementService.GetLatestAgreementId");
            throw;
        }
    }

    private async Task<LegalEntityViewModel> GetLegalEntity(long accountId, long maLegalEntityId)
    {
        var hashedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);
        return await _accountApiClient.GetLegalEntity(hashedAccountId, maLegalEntityId);
    }
}