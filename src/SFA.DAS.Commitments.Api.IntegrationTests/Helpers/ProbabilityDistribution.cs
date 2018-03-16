using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
{
    public class ProbabilityDistribution<T>
    {
        public class BoundaryValue
        {
            public int Boundary { get; set; }
            public Func<T> Value { get; set; }

            public BoundaryValue(int boundary, Func<T> value)
            {
                Boundary = boundary;
                Value = value;
            }
        }

        private readonly IEnumerable<BoundaryValue> _boundaryValues;
        private readonly int _range;
        private readonly Random _random = new Random();

        public ProbabilityDistribution(IEnumerable<BoundaryValue> boundaryValues)
        {
            _boundaryValues = boundaryValues;
            _range = boundaryValues.Last().Boundary;
        }

        public T NextRandom()
        {
            var rand = _random.Next(_range);
            return _boundaryValues.First(bv => rand < bv.Boundary).Value();
        }

        //public T LowerBound => _boundaryValues.First().Value();
    }
}
