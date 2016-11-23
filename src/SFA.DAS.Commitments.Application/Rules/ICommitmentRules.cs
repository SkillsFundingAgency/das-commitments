namespace SFA.DAS.Commitments.Application.Rules
{
    using System.Collections.Generic;

    using Api.Types;

    using Apprenticeship = SFA.DAS.Commitments.Domain.Entities.Apprenticeship;

    public interface ICommitmentRules
    {
        AgreementStatus DetermineAgreementStatus(List<Apprenticeship> apprenticeships);
    }
}