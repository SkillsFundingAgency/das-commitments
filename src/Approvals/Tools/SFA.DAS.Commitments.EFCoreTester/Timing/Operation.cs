using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;

namespace SFA.DAS.Commitments.EFCoreTester.Timing
{
    public class Operation : IOperation
    {
        private static int _id;

        private readonly List<IOperation> _childOperations;

        public Operation(string title, int level)
        {
            Title = title;
            Level = level;
            Started = DateTime.Now;
            _childOperations = new List<IOperation>();
            Id = Interlocked.Increment(ref _id);
        }

        public int Id { get; }
        public string Title { get; }
        public int Level { get; }
        public DateTime Started { get; }
        public DateTime Ended { get; set; }
        public TimeSpan Elapsed => Ended - Started;
        public IReadOnlyCollection<IOperation> ChildOperations => _childOperations;

        public void AddChildOperation(IOperation operation)
        {
            _childOperations.Add(operation);
        }

        public IEnumerable<OperationAggregation> GetSummary()
        {
            return Flatten(this)
                .GroupBy(operation => operation.Title)
                .Select(grp =>
                    new OperationAggregation(grp.Key,
                        grp.OrderBy(op => op.Id).Select(op => op.Elapsed.TotalMilliseconds)));
        }

        private IEnumerable<IOperation> Flatten(IOperation operation)
        {
            yield return operation;

            foreach (var childOperation in operation.ChildOperations)
            {
                foreach (var operation1 in Flatten(childOperation))
                {
                    yield return operation1;
                }
            }
        }


    }
}