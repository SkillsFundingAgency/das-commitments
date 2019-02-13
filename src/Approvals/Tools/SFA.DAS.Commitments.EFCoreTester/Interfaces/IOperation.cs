using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.EFCoreTester.Timing;

namespace SFA.DAS.Commitments.EFCoreTester.Interfaces
{
    public interface IOperation
    {
        int Id { get; }
        string Title { get; }
        int Level { get; }
        DateTime Started { get; }
        DateTime Ended { get; }
        TimeSpan Elapsed { get; }
        IReadOnlyCollection<IOperation> ChildOperations { get; }
        IEnumerable<OperationAggregation> GetSummary();
    }
}