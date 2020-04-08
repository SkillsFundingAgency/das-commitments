using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ChangeOfPartyRequestValidatorTests
    {
        [TestCase(null, false)]
        [TestCase(0, true)]
        [TestCase(1, true)]
        public void Validate_NewPrice_ShouldBeValidated(int? newPrice, bool isValid)
        {
            AssertValidationResult(r => r.NewPrice, newPrice, isValid);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void Validate_NewStartDate_ShouldBeValidated(bool isSet, bool isValid)
        {
            AssertValidationResult(r => r.NewStartDate, isSet ? DateTime.Today : (DateTime?)null, isValid);
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_NewPartyId_ShouldBeValidated(long newPartyId, bool isValid)
        {
            AssertValidationResult(r => r.NewPartyId, newPartyId, isValid);
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_ApprenticeshipId_ShouldBeValidated(long apprenticeshipId, bool isValid)
        {
            AssertValidationResult(r => r.ApprenticeshipId, apprenticeshipId, isValid);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void Validate_UserInfo_ShouldBeValidated(bool isSet, bool isValid)
        {
            AssertValidationResult(r => r.UserInfo, isSet ? new UserInfo() : null, isValid);
        }

        private void AssertValidationResult<T>(Expression<Func<ChangeOfPartyRequestCommand, T>> property, T value, bool isValid)
        {
            var validator = new ChangeOfPartyRequestValidator();
            
            if (isValid)
            {
                validator.ShouldNotHaveValidationErrorFor(property, value);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(property, value);
            }
        }
    }
}