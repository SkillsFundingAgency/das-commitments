using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public enum AgreementStatus : short
    {
        NotAgreed = 0,
        EmployerAgreed = 1,
        ProviderAgreed = 2,
        BothAgreed = 3
    }
}