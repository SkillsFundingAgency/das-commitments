using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Validators;
using SFA.DAS.Reservations.Api.Client;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models
{
    [TestFixture]
    [Parallelizable]
    public class AddDraftApprenticeshipModelValidatorTests
    {
        private AddDraftApprenticeshipModelValidatorTestFixtures _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddDraftApprenticeshipModelValidatorTestFixtures();
        }

        [Test]
        public void ReservationId_CheckReservationIsValid_IsValid()
        {
            _fixture.DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                ReservationId = null
            };

            _fixture.AssertValidationForProperty(() => { },
                draftApprenticeshipDetails => draftApprenticeshipDetails.DraftApprenticeshipDetails.ReservationId,
                true);
        }

        [Test]
        public void ReservationId_CheckReservationIsValid_IsNotValid()
        {
            _fixture.DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                ReservationId = Guid.NewGuid()
            };

            var propertyNamesToReportAsErrorsInReservations = new string[]
            {
                nameof(ValidationReservationMessage.ReservationId),
                nameof(ValidationReservationMessage.AccountId),
                nameof(ValidationReservationMessage.StartDate)
            };

            _fixture.AssertPropertiesHaveValidationErrors(
                () => _fixture.WithUnsuccessfulReservationValidation(propertyNamesToReportAsErrorsInReservations),
                propertyNamesToReportAsErrorsInReservations);
        }
    }

    public class AddDraftApprenticeshipModelValidatorTestFixtures

    {
        public DraftApprenticeshipDetails DraftApprenticeshipDetails;
        public Commitment Cohort;
        public Mock<IReservationsApiClient> ReservationsApiClient;
        public Mock<IValidator<DraftApprenticeshipDetails>> DraftApprenticeshipDetailsValidator;

        public AddDraftApprenticeshipModelValidatorTestFixtures()
        {
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue)
            };
            Cohort = new Commitment();
            ReservationsApiClient = new Mock<IReservationsApiClient>();
            DraftApprenticeshipDetailsValidator = new Mock<IValidator<DraftApprenticeshipDetails>>();

            DraftApprenticeshipDetailsValidator
                .Setup(da => da.Validate(It.IsAny<ValidationContext>()))
                .Returns(new FluentValidation.Results.ValidationResult());

            DraftApprenticeshipDetailsValidator
                .Setup(da => da.Validate(It.IsAny<DraftApprenticeshipDetails>()))
                .Returns(new FluentValidation.Results.ValidationResult());

            DraftApprenticeshipDetailsValidator
                .Setup(da => da.ValidateAsync(It.IsAny<DraftApprenticeshipDetails>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        }

        public AddDraftApprenticeshipModelValidatorTestFixtures WithSuccessfulReservationValidation()
        {
            ReservationsApiClient.Setup(rac =>
                    rac.ValidateReservation(It.IsAny<ValidationReservationMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult {ValidationErrors = new ValidationError[0]});

            return this;
        }

        public AddDraftApprenticeshipModelValidatorTestFixtures WithUnsuccessfulReservationValidation(params string[] invalidProperties)
        {
            var errors = invalidProperties.Select(propertyName => new ValidationError
            {
                Code = "ERR01",
                PropertyName = propertyName,
                Reason = $"{propertyName} is invalid"
            }).ToArray();

            ReservationsApiClient.Setup(rac =>
                    rac.ValidateReservation(It.IsAny<ValidationReservationMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult { ValidationErrors = errors});

            return this;
        }

        public void AssertValidationForProperty<TValue>(Action setup, Expression<Func<AddDraftApprenticeshipModel, TValue>> expression, bool passes)
        {
            setup();

            var validator = new AddDraftApprenticeshipModelValidator(DraftApprenticeshipDetailsValidator.Object, ReservationsApiClient.Object);
            var model = new AddDraftApprenticeshipModel(Cohort, DraftApprenticeshipDetails);

            if (passes)
            {
                validator.ShouldNotHaveValidationErrorFor(expression, model);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(expression, model);
            }
        }

        public void AssertPropertiesHaveValidationErrors(Action setup, string[] propertyNames)
        {
            setup();

            var validator = new AddDraftApprenticeshipModelValidator(DraftApprenticeshipDetailsValidator.Object, ReservationsApiClient.Object);

            var model = new AddDraftApprenticeshipModel(Cohort, DraftApprenticeshipDetails);

            var validationResults = validator.TestValidate(model, null);

            foreach (var propertyName in propertyNames)
            {
                var errorsForProperty = validationResults.Result.Errors
                    .Where(error =>string.Equals(error.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (errorsForProperty.Length == 0)
                {
                    Assert.Fail($"Did not get validation error for property {propertyName}");
                }
                else
                {
                    Console.WriteLine($"Found {errorsForProperty.Length} error(s) for property {propertyName} as expected - test passed");
                }
            }
        }
    }
}