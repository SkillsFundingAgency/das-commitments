using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Validators;
using SFA.DAS.Learners.Validators;
using IUlnValidator = SFA.DAS.Learners.Validators.IUlnValidator;
using UlnValidationResult = SFA.DAS.CommitmentsV2.Domain.Entities.UlnValidationResult;
using UlnValidator = SFA.DAS.CommitmentsV2.Services.UlnValidator;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class UlnValidatorTests
    {
        [TestCase(UlnValidationResult.Success)]
        [TestCase(UlnValidationResult.IsEmptyUlnNumber)]
        [TestCase(UlnValidationResult.IsInValidTenDigitUlnNumber)]
        [TestCase(UlnValidationResult.IsInvalidUln)]
        public void Validate_Uln(UlnValidationResult validationResult)
        {
            //Arrange
            Mock<IUlnValidator> ulnValidatorMock = new Mock<IUlnValidator>();
            ulnValidatorMock.Setup(x => x.Validate(It.IsAny<string>()))
                .Returns((Learners.Validators.UlnValidationResult) validationResult);

            var ulnValidator = new UlnValidator(ulnValidatorMock.Object);

            //Act
            var result = ulnValidator.Validate("");

            //Assert
            Assert.AreEqual(result, validationResult);
        }
    }
}