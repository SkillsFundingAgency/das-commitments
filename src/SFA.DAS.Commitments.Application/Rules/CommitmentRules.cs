using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Api.Types;
using Apprenticeship = SFA.DAS.Commitments.Domain.Entities.Apprenticeship;

namespace SFA.DAS.Commitments.Application.Rules
{
    public class CommitmentRules : ICommitmentRules
    {
        public AgreementStatus DetermineAgreementStatus(List<Apprenticeship> apprenticeships)
        {
            var first = apprenticeships?.FirstOrDefault();

            if (first == null)
            {
                return AgreementStatus.NotAgreed;
            }

            return (AgreementStatus) first.AgreementStatus;
        }
    }
}
