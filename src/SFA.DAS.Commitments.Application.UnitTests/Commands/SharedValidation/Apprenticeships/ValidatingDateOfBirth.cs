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
            MockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2017, 06, 10));
            ExampleValidApprenticeship.StartDate = new DateTime(2017, 08, 01);
            ExampleValidApprenticeship.DateOfBirth = new DateTime(2003, 04, 01);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(2000, 4, 1)]
        [TestCase(2002, 8, 1)]
        [TestCase(2002, 7, 31)]
        public void ShouldBeValidIfOlderThan15OnStartDate(int year, int month, int day)
        {
            var dob = new DateTime(year, month, day);

            MockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2017, 6, 10));
            ExampleValidApprenticeship.StartDate = new DateTime(2017, 8, 1);
            ExampleValidApprenticeship.DateOfBirth = dob;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeTrue();
        }
    }
}
