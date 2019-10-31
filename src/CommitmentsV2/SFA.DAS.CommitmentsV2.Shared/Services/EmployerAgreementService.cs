using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Models;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Shared.Services
{
    public class EmployerAgreementService : IEmployerAgreementService
    {
        private readonly IAccountApiClient _accountApiClient;
        private readonly ICommitmentsApiClient _commitmentsApiClient;
        private readonly IEncodingService _encodingService;
        private readonly ILogger<EmployerAgreementService> _logger;

        private readonly Dictionary<AgreementFeature, int> _agreementUnlocks;

        public EmployerAgreementService(IAccountApiClient accountApiClient, ICommitmentsApiClient commitmentsApiClient, IEncodingService encodingService, ILogger<EmployerAgreementService> logger)
        {
            _accountApiClient = accountApiClient;
            _commitmentsApiClient = commitmentsApiClient;
            _encodingService = encodingService;
            _logger = logger;

            //This dictionary indicates which features are unlocked by agreement versions
            _agreementUnlocks = new Dictionary<AgreementFeature, int> { { AgreementFeature.Transfers, 2 } };
        }

        public async Task<bool> IsAgreementSigned(long accountId, long accountLegalEntityId, CancellationToken cancellationToken = default(CancellationToken), 
            params AgreementFeature[] requiredFeatures)
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
                var legalEntity = await GetLegalEntity(accountId, accountLegalEntityId, cancellationToken);

                var signedAgreements = legalEntity.Agreements
                    .Where(x => x.Status == EmployerAgreementStatus.Signed).ToList();

                if (!signedAgreements.Any())
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
                _logger.LogError(e, $"Error in EmployerAgreementService.IsAgreementSigned: {e.Message}");
                throw;
            }
        }

        public async Task<long?> GetLatestAgreementId(long accountId, long accountLegalEntityId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
               var legalEntity = await GetLegalEntity(accountId, accountLegalEntityId, cancellationToken);

               var latestAgreement = legalEntity.Agreements.OrderByDescending(x => x.TemplateVersionNumber).FirstOrDefault();

                if (latestAgreement is null)
                {
                    return null;
                }

                return latestAgreement.Id;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in EmployerAgreementService.GetLatestAgreementId: {e.Message}");
                throw;
            }
        }

        private async Task<LegalEntityViewModel> GetLegalEntity(long accountId, long accountLegalEntityId, CancellationToken cancellationToken)
        {
            var maLegalEntityId = (await _commitmentsApiClient.GetLegalEntity(accountLegalEntityId, cancellationToken)).MaLegalEntityId;
            var hashedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);
            var legalEntity = await _accountApiClient.GetLegalEntity(hashedAccountId, maLegalEntityId);
            return legalEntity;
        }
    }
}
