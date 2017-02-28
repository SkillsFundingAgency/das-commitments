using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingFamilyName : ApprenticeshipValidationTestBase
    {
        [TestCase("")]
        [TestCase(null)]
        public void ThenApprenticeshipWithoutLastNameIsNotSet(string lastName)
        {
            ExampleValidApprenticeship.LastName = lastName;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }
    }
}
