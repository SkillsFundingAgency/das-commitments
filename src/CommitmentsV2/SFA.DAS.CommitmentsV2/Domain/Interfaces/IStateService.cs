using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IStateService
    {
        IReadOnlyList<DiffItem> GenerateDiff(Dictionary<string, object> initial, Dictionary<string, object> updated);
        Dictionary<string, object> GetState(object item);
    }
}
