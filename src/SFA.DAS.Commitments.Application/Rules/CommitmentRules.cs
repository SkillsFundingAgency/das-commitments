namespace SFA.DAS.Commitments.Application.Rules
{
    using System.Collections.Generic;
    using System.Linq;

    using Api.Types;
    
    using Apprenticeship = SFA.DAS.Commitments.Domain.Entities.Apprenticeship;

    public class CommitmentRules : ICommitmentRules
    {
        public AgreementStatus DetermineAgreementStatus(List<Apprenticeship> apprenticeships)
        {
            var first = apprenticeships?.FirstOrDefault();
            if (first == null)
            {
                return AgreementStatus.NotAgreed;
            }
            return (AgreementStatus)first.AgreementStatus;
        }
    }
}