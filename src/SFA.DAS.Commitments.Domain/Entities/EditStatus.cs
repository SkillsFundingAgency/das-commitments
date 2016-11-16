using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public enum EditStatus
    {
        Both = 0,
        EmployerOnly = 1,
        ProviderOnly = 2,
        Neither = 3
    }
}
