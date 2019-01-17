using System;
using System.Linq;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;

namespace SFA.DAS.Commitments.EFCoreTester.Timing
{
    public class OperationTimingOutputter : IOperationTimingOutputter
    {
        public void ShowLog(IOperation operation)
        {
            Console.WriteLine("Operation (root down to sub-operations) - all times msecs");
            Console.WriteLine("=========================================================");

            ShowLogInternal(operation);
        }

        public void ShowSummary(IOperation operation)
        {
            Console.WriteLine("");
            Console.WriteLine("Operation (one line per operation type) - all times msecs");
            Console.WriteLine("=========================================================");

            var aggregates = operation.GetSummary();

            const int WarmUpTime = 2;

            Console.WriteLine("Title total Warm-Elapsed Warm-Avg Runs");

            foreach (var aggregate in aggregates)
            {
                var allElapsed = aggregate.Durations.Sum();
                var afterWarmUpElapsed = aggregate.Durations.Skip(WarmUpTime).Sum();
                var afterWarmUpAverage = aggregate.Durations.Length > WarmUpTime ? aggregate.Durations.Skip(WarmUpTime).Average() : aggregate.Durations.Average();

                Console.Write($"{aggregate.Title.Replace(' ','_'),-20} {allElapsed:F3} {afterWarmUpElapsed:F3} {afterWarmUpAverage:F3} {aggregate.Durations.Length}");

                foreach (var duration in aggregate.Durations)
                {
                    Console.Write($" {duration:F3}");
                }
                Console.WriteLine();
            }
        }

        private void ShowLogInternal(IOperation operation)
        {
            var padding = new string(' ', operation.Level * 4);
            Console.WriteLine($"{operation.Id:000}: {padding} {operation.Title,-20} {operation.Elapsed.TotalMilliseconds:F3} {operation.ChildOperations.Count}");

            foreach (var childOperation in operation.ChildOperations)
            {
                ShowLog(childOperation);
            }
        }
    }
}