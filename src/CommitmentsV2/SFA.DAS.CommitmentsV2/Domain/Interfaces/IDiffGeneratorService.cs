using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IDiffGeneratorService
    {
        IReadOnlyList<DiffItem> GenerateDiff(object initial, object updated);
    }
}
