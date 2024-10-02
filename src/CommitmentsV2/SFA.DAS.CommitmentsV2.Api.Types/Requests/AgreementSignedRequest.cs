using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class AgreementSignedRequest
{
    public long AccountLegalEntityId { get; set; }
    public AgreementFeature[] AgreementFeatures { get; set; }
}