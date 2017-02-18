using System;
using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingDateOfBirth : ApprenticeshipValidationTestBase
    {
        [Test]
        public void ShouldBeInvalidIfYoungerThan15OnStartDate()
        {
            ExampleValidApprenticeship.StartDate = DateTime.Now.AddDays(30);
            ExampleValidApprenticeship.DateOfBirth = DateTime.Now.AddYears(-13);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ShouldBeValidIfOlderThan15OnStartDate()
        {
            ExampleValidApprenticeship.StartDate = DateTime.Now.AddDays(30);
            ExampleValidApprenticeship.DateOfBirth = DateTime.Now.AddYears(-15);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeTrue();
        }
    }
}
