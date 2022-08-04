using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class CreateChangeOfPartyRequestValidatorTests
    {
        private Mock<IAcademicYearDateProvider> _mockAcademicYearDateProvider;
        private DateTime _currentAcademicYearEndDate;

        [SetUp]
        public void Arrange()
        {
            var academicEndYear = DateTime.UtcNow.Month > 7 ? DateTime.UtcNow.AddYears(1).Year : DateTime.UtcNow.Year;
            _currentAcademicYearEndDate = new DateTime(academicEndYear, 7, 31);

            _mockAcademicYearDateProvider = new Mock<IAcademicYearDateProvider>();
            _mockAcademicYearDateProvider.Setup(p => p.CurrentAcademicYearEndDate).Returns(_currentAcademicYearEndDate);
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

        [TestCase(null, true)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(100000, true)]
        [TestCase(100001, false)]
        public void Validate_NewPrice_WhenNewPriceHasValue_ThenShouldBeGreaterThanZeroAndLessThanOrEqualTo100000(int? price, bool isValid)
        {
            AssertValidationResult(r => r.NewPrice, price, isValid);
        }

        [TestCase(false, 1, false, false)]
        [TestCase(false, null, true, false)]
        [TestCase(false, 1, true, false)]
        [TestCase(false, null, false, true)]
        [TestCase(true, 1, true, true)]
        public void Validate_NewStartDate_WhenNewPriceOrNewEndDateHasValue(bool hasNewStartDate, int? newPrice, bool hasNewEndDate, bool isValid)
        {
            var command = new CreateChangeOfPartyRequestCommand
            {
                NewStartDate = hasNewStartDate ? new DateTime(2020, 1, 1) : (DateTime?)null,
                NewEndDate = hasNewEndDate ? new DateTime(2022, 1, 1) : (DateTime?)null,
                NewPrice = newPrice
            };

            AssertValidationResult(r => r.NewStartDate, command, isValid);
        }

        [TestCase(false, false, 1, false)]
        [TestCase(false, true, null, false)]
        [TestCase(false, true, 1, false)]
        [TestCase(false, false, null, true)]
        [TestCase(true, true, 1, true)]
        public void Validate_NewEndDate_WhenNewStartDateOrNewPriceHasValue(bool hasNewEndDate, bool hasNewStartDate, int? newPrice,  bool isValid)
        {
            var command = new CreateChangeOfPartyRequestCommand
            {
                NewStartDate = hasNewStartDate ? new DateTime(2020, 1, 1) : (DateTime?)null,
                NewEndDate = hasNewEndDate ? new DateTime(2022, 1, 1) : (DateTime?)null,
                NewPrice = newPrice
            };

            AssertValidationResult(r => r.NewEndDate, command, isValid);
        }

        [TestCase(null, false, true, false)]
        [TestCase(null, true, false, false)]
        [TestCase(null, true, true, false)]
        [TestCase(null, false, false, true)]
        [TestCase(1, true, true, true)]
        public void Validate_NewPrice_WhenNewStartDateOrNewEndDateHasValue(int? newPrice, bool hasNewStartDate, bool hasNewEndDate, bool isValid)
        {
            var command = new CreateChangeOfPartyRequestCommand
            {
                NewStartDate = hasNewStartDate ? new DateTime(2020, 1, 1) : (DateTime?)null,
                NewEndDate = hasNewEndDate ? new DateTime(2022, 1, 1) : (DateTime?)null,
                NewPrice = newPrice
            };

            AssertValidationResult(r => r.NewPrice, command, isValid);
        }

        [Test]
        public void Validate_NewStartDate_WhenNewStartDateIsBeforeNewEndDate_ThenIsValid()
        {
            var command = new CreateChangeOfPartyRequestCommand
            {
                NewStartDate = DateTime.Today,
                NewEndDate = DateTime.Today.AddYears(1)
            };

            AssertValidationResult(r => r.NewStartDate, command, true);
        }

        [Test]
        public void Validate_NewStartDate_WhenNewStartDateIsEqualToNewEndDate_ThenIsInvalid()
        {
            var command = new CreateChangeOfPartyRequestCommand
            {
                NewStartDate = DateTime.Today,
                NewEndDate = DateTime.Today
            };

            AssertValidationResult(r => r.NewStartDate, command, false);
        }

        [Test]
        public void Validate_NewStartDate_WhenNewStartDateIsAfterNewEndDate_ThenIsInvalid()
        {
            var command = new CreateChangeOfPartyRequestCommand
            {
                NewStartDate = DateTime.Today.AddYears(1),
                NewEndDate = DateTime.Today
            };

            AssertValidationResult(r => r.NewStartDate, command, false);
        }

        [Test]
        public void Validate_NewStartDate_WhenNewStartDateIsAfterCurrentAcademicYearPlusOneYear_ThenIsInvalid()
        {
            var command = new CreateChangeOfPartyRequestCommand
            {
                NewStartDate = _currentAcademicYearEndDate.AddYears(1).AddDays(1),
                NewEndDate = _currentAcademicYearEndDate.AddYears(2)
            };

            AssertValidationResult(r => r.NewStartDate, command, false);
        }

        private void AssertValidationResult<T>(Expression<Func<CreateChangeOfPartyRequestCommand, T>> property, T value, bool isValid)
        {
            var validator = new CreateChangeOfPartyRequestValidator(_mockAcademicYearDateProvider.Object);
            
            if (isValid)
            {
                validator.ShouldNotHaveValidationErrorFor(property, value);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(property, value);
            }
        }

        private void AssertValidationResult<T>(Expression<Func<CreateChangeOfPartyRequestCommand, T>> property, CreateChangeOfPartyRequestCommand command , bool isValid)
        {
            var validator = new CreateChangeOfPartyRequestValidator(_mockAcademicYearDateProvider.Object);

            if (isValid)
            {
                validator.ShouldNotHaveValidationErrorFor(property, command);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(property, command);
            }
        }
    }
}