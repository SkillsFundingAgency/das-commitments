using System;
using System.Globalization;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class CreateCohortRequestValidatorTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("Fred Flintstone", true)]
        public void Validate_UserId_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.UserId, value, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_AccountLegalEntityId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.AccountLegalEntityId, value, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_ProviderId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.ProviderId, value, expectedValid);
        }


        [TestCase("2019-04-01", "2019-03-01", false)]
        [TestCase("2019-04-01", "2019-04-01", false)]
        [TestCase("2019-04-01", "2019-05-01", true)]
        public void Validate_EndDateCode_ShouldBeValidated(string startDateValue, string endDateValue, bool expectedValid)
        {
            const string requiredDateFormat = "yyyy-MM-dd";

            var startDate = DateTime.ParseExact(startDateValue, requiredDateFormat, CultureInfo.CurrentCulture);
            var endDate = DateTime.ParseExact(endDateValue, requiredDateFormat, CultureInfo.CurrentCulture);

            var requestInstance = new CreateCohortRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };

            AssertValidationResult(request => request.EndDate, requestInstance, expectedValid);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void Validate_ReservationId_ShouldBeValidated(bool useBlankGuid, bool expectedValid)
        {
            var guidToUse = useBlankGuid ? Guid.Empty : Guid.NewGuid();

            AssertValidationResult(request => request.ReservationId, guidToUse, expectedValid);
        }

        [TestCase(null, false)]
        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(100000, true)]
        [TestCase(100001, false)]
        public void Validate_Cost_ShouldBeValidated(int? cost, bool expectedValid)
        {
           AssertValidationResult(request => request.Cost, cost, expectedValid);
        }

        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase("                                                           ", true)]
        [TestCase("A long string that is much longer than the allowed 20 chars", false)]
        [TestCase("A", true)]
        [TestCase("....:....|....:....|", true)]
        public void Validate_OriginatorReference_ShouldBeValidated(string originatorReference, bool expectedValid)
        {
            AssertValidationResult(request => request.OriginatorReference, originatorReference, expectedValid);
        }

        private void AssertValidationResult<T>(Expression<Func<CreateCohortRequest,T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new CreateCohortRequestValidator();

            // Act
            if (expectedValid)
            {
                validator.ShouldNotHaveValidationErrorFor(property, value);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(property, value);
            }
        }

        private void AssertValidationResult<T>(Expression<Func<CreateCohortRequest, T>> property, CreateCohortRequest instance, bool expectedValid)
        {
            // Arrange
            var validator = new CreateCohortRequestValidator();

            // Act
            if (expectedValid)
            {
                validator.ShouldNotHaveValidationErrorFor(property, instance);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(property, instance);
            }
        }
    }
}
