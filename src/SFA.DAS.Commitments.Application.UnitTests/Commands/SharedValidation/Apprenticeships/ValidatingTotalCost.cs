using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingTotalCost : ApprenticeshipValidationTestBase
    {
        [TestCase(123.12)]
        [TestCase(123.1)]
        [TestCase(123.0)]
        [TestCase(123)]
        [TestCase(123.000)]
        public void ThenCostThatIsNumericAndHas2DecimalPlacesIsValid(decimal cost)
        {
            ExampleValidApprenticeship.Cost = cost;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeTrue();
        }

        [TestCase(123.1232)]
        [TestCase(0.001)]
        public void ThenCostThatIsNotAMax2DecimalPlacesIsInvalid(decimal cost)
        {
            ExampleValidApprenticeship.Cost = cost;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-0)]
        [TestCase(-123.12)]
        [TestCase(-123)]
        [TestCase(-123.1232)]
        [TestCase(-0.001)]
        public void ThenCostThatIsZeroOrNegativeNumberIsInvalid(decimal cost)
        {
            ExampleValidApprenticeship.Cost = cost;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenCostGreaterThan100000IsInvalid()
        {
            ExampleValidApprenticeship.Cost = 100001;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenCostEqualTo100000IsValid()
        {
            ExampleValidApprenticeship.Cost = 100000;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeTrue();
        }
    }
}
