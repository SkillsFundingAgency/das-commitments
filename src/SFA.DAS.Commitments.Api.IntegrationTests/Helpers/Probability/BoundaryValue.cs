using System;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers.Probability
{
    public class BoundaryValue<T>
    {
        public int Boundary { get; set; }
        public Func<T> Value { get; set; }

        public BoundaryValue(int boundary, Func<T> value)
        {
            Boundary = boundary;
            Value = value;
        }
    }
}
