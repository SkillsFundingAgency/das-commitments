using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class EditEndDateRequestValidatorTests
    {
        [Test]
        public void InvalidEndDateValidationFails()
        {

            var model = new EditEndDateRequest { EndDate = null};
            AssertValidationResult(request => request.EndDate, model, false);
        }

        [Test]
        public void ValidEndDateValidationPasses()
        {

            var model = new EditEndDateRequest { EndDate = DateTime.Now };
            AssertValidationResult(request => request.EndDate, model, true);
        }

        [TestCase(5143541, true)]
        [TestCase(0, false)]
        [TestCase(null, false)]
        public void ThenApprenticeshipIdIsValidated(long apprenticeshipId, bool expectedValid)
        {
            var viewModel = new EditEndDateRequest() { ApprenticeshipId = apprenticeshipId };
            AssertValidationResult(x => x.ApprenticeshipId, viewModel, expectedValid);
        }

        private static void AssertValidationResult<T>(Expression<Func<EditEndDateRequest, T>> property, EditEndDateRequest instance, bool expectedValid)
        {
            var validator = new EditEndDateRequestValidator();

            var result = validator.TestValidate(instance);
            
            if (expectedValid)
            {
                result.ShouldNotHaveValidationErrorFor(property);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(property);
            }
        }
    }
}
