using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitmentAgreements
{
    [TestFixture]
    public class WhenGettingCommitmentAgreements
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<AbstractValidator<GetCommitmentAgreementsRequest>> _mockValidator;
        private GetCommitmentAgreementsQueryHandler _handler;
        private GetCommitmentAgreementsRequest _exampleValidRequest;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockValidator = new Mock<AbstractValidator<GetCommitmentAgreementsRequest>>();
            _handler = new GetCommitmentAgreementsQueryHandler(_mockCommitmentRespository.Object, _mockValidator.Object);

            _exampleValidRequest = new GetCommitmentAgreementsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 1
                }
            };
        }

        /// <remarks>
        /// WhenValidatingGetCommitmentAgreementsRequest tests the specific validator currently used, here we just check that the query handler handles validation failures correctly
        /// </remarks>
        [Test]
        public void ThenInvalidRequestShouldThrowValidationExceptionWithValidationErrors()
        {
            const string errorPropertyName = "propertyName", errorError = "error";

            _mockValidator.Setup(v => v.Validate(It.IsAny<GetCommitmentAgreementsRequest>()))
                .Returns(new ValidationResult(new [] {new ValidationFailure(errorPropertyName, errorError) }));

            var validationException = Assert.ThrowsAsync<ValidationException>(async () =>  await _handler.Handle(_exampleValidRequest));
            Assert.AreEqual(1, validationException.Errors.Count());
            Assert.AreEqual(errorPropertyName, validationException.Errors.First().PropertyName);
            Assert.AreEqual(errorError, validationException.Errors.First().ErrorMessage);
        }

        [Test]
        public void ThenValidRequestShouldNotThrowValidationExceptionWithValidationErrors()
        {
            _mockValidator.Setup(v => v.Validate(It.IsAny<GetCommitmentAgreementsRequest>()))
                .Returns(new ValidationResult(Enumerable.Empty<ValidationFailure>()));

            Assert.That(async () => await _handler.Handle(_exampleValidRequest), Throws.Nothing);
        }
    }
}
