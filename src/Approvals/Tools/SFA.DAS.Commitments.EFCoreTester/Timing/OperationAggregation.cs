using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.EFCoreTester.Timing
{
    public class OperationAggregation
    {
        public OperationAggregation(string title, IEnumerable<double> durations)
        {
            Title = title;
            Durations = durations.ToArray();
        }

        public string Title { get; }
        public double[] Durations { get; }
    }
}
