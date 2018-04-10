using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers.Probability
{
    public class ProbabilityDistribution<T>
    {
        private readonly IEnumerable<BoundaryValue<T>> _boundaryValues;
        private readonly int _range;
        private readonly Random _random = new Random();

        public ProbabilityDistribution(IEnumerable<BoundaryValue<T>> boundaryValues)
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
