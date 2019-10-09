using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IEncodingService _encodingService;

        private readonly Dictionary<AgreementFeature, int> _agreementUnlocks;

        public EmployerAgreementService(IAccountApiClient accountApiClient, IEncodingService encodingService)
        {
            _accountApiClient = accountApiClient;
            _encodingService = encodingService;

            //This dictionary indicates which features are unlocked by agreement versions
            _agreementUnlocks = new Dictionary<AgreementFeature, int> { { AgreementFeature.Transfers, 2 } };
        }

        public async Task<bool> IsAgreementSigned(long accountId, long accountLegalEntityId,  params AgreementFeature[] requiredFeatures)
        {
            bool AreAllRequiredFeaturesPresentInSignedAgreement(int signedAgreement)
            {
                if (requiredFeatures.Any(f => signedAgreement < _agreementUnlocks[f]))
                {
                    return false;
                }

                return true;
            }

            var hashedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);
            var legalEntity = await _accountApiClient.GetLegalEntity(hashedAccountId, accountLegalEntityId);

            var signedAgreements = legalEntity.Agreements
                .Where(x => x.Status == EmployerAgreementStatus.Signed).ToList();

            if (!signedAgreements.Any())
            {
                return false;
            }

            //No extended features required
            if (requiredFeatures.Length == 0)
            {
                return true;
            }

            return AreAllRequiredFeaturesPresentInSignedAgreement(signedAgreements.Max(x => x.TemplateVersionNumber));
        }
    }
}
