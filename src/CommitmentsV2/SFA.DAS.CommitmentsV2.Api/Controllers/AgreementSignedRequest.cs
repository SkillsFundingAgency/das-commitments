using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    public class AgreementSignedRequest
    {
        public long AccountLegalEntityId { get; set; }
        public AgreementFeature[] AgreementFeatures { get; set; }
    }
}