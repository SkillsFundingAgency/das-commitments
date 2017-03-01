using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingGivenName : ApprenticeshipValidationTestBase
    {
        [TestCase("")]
        [TestCase(null)]
        public void ThenApprenticeshipWithoutFirstNameIsNotSet(string firstName)
        {
            ExampleValidApprenticeship.FirstName = firstName;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }
    }
}
