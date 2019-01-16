using System;
using System.Linq;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;

namespace SFA.DAS.Commitments.EFCoreTester.Timing
{
    public class OperationTimingOutputter : IOperationTimingOutputter
    {
        public void Show(IOperation operation)
        {
            ShowLog(operation);
            ShowSummary(operation);
        }

        public void ShowLog(IOperation operation)
        {
            var padding = new string(' ', operation.Level * 4);
            Console.WriteLine($"{operation.Id:000}: {padding} {operation.Title,-20} {operation.Elapsed.TotalMilliseconds:F3} {operation.ChildOperations.Count}");

            foreach (var childOperation in operation.ChildOperations)
            {
                Show(childOperation);
            }
        }

        public void ShowSummary(IOperation operation)
        {
            var aggregates = operation.GetSummary();

            foreach (var aggregate in aggregates)
            {
                var allElapsed = aggregate.Durations.Sum();
                var afterWarmUpElapsed = aggregate.Durations.Skip(1).Sum();

                Console.Write($"{aggregate.Title,-20} {allElapsed:F3} {afterWarmUpElapsed:F3}");
                foreach (var duration in aggregate.Durations)
                {
                    Console.Write($" {duration:F3}");
                }
                Console.WriteLine();
            }
        }
    }
}