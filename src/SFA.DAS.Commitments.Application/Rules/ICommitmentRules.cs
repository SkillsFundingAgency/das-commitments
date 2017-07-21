using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    using System.Collections.Generic;
    using Apprenticeship = Domain.Entities.Apprenticeship;

    public interface ICommitmentRules
    {
        AgreementStatus DetermineAgreementStatus(List<Apprenticeship> apprenticeships);
    }
}