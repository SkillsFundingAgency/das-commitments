using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using Moq;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingTrainingDates : ApprenticeshipValidationTestBase
    {
        [Test]
        public void ShouldBeInvalidIfStartDateBeforeMay2017()
        {
            ExampleValidApprenticeship.StartDate = new DateTime(2017, 4, 22);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ShouldBeInvalidIfEndDateBeforeStartDate()
        {
            ExampleValidApprenticeship.StartDate = new DateTime(2017, 7, 22);
            ExampleValidApprenticeship.EndDate = new DateTime(2017, 6, 28);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ShouldBeInvalidIfEndDateIsTheSameAsStartDate()
        {
            ExampleValidApprenticeship.StartDate = new DateTime(2017, 7, 22);
            ExampleValidApprenticeship.EndDate = new DateTime(2017, 7, 22);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ShouldBeInvalidIfEndDateIsInThePast()
        {
            ExampleValidApprenticeship.StartDate = new DateTime(2017, 5, 10);
            ExampleValidApprenticeship.EndDate = new DateTime(2017, 5, 22);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ShouldBeInvalidIfAcademicYearValidatorIsNotSucessful()
        {
            ExampleValidApprenticeship.StartDate = new DateTime(2017, 7, 22);
            MockAcademicYearValidator
                .Setup(x => x.Validate(ExampleValidApprenticeship.StartDate.Value))
                .Returns(AcademicYearValidationResult.NotWithinFundingPeriod);

            var result = Validator.Validate(ExampleValidApprenticeship);
            MockAcademicYearValidator.Verify(x => x.Validate(ExampleValidApprenticeship.StartDate.Value), Times.AtLeastOnce);

            result.IsValid.Should().BeFalse();
        }

    }
}
