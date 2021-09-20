using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetProviderCommitmentAgreementResponse
    {
        public List<ProviderCommitmentAgreement> ProviderCommitmentAgreement { get; set; }
    }
}
