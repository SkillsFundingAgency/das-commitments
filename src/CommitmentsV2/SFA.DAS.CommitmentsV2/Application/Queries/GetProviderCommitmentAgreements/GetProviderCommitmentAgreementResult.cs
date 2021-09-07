using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements
{
    public class GetProviderCommitmentAgreementResult
    {
        public List<ProviderCommitmentAgreement> Agreements { get; }

        public GetProviderCommitmentAgreementResult(IEnumerable<ProviderCommitmentAgreement> agreements)
        {
            Agreements = agreements.ToList();
        }
    }
}
