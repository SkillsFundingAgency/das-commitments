using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

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
            _handler = new GetCommitmentAgreementsQueryHandler(_mockCommitmentRespository.Object,
                _mockValidator.Object);

            _mockValidator.Setup(v => v.Validate(It.IsAny<GetCommitmentAgreementsRequest>()))
                .Returns(new ValidationResult(Enumerable.Empty<ValidationFailure>()));

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
                .Returns(new ValidationResult(new[] {new ValidationFailure(errorPropertyName, errorError)}));

            var validationException =
                Assert.ThrowsAsync<ValidationException>(async () => await _handler.Handle(_exampleValidRequest));
            Assert.AreEqual(1, validationException.Errors.Count());
            Assert.AreEqual(errorPropertyName, validationException.Errors.First().PropertyName);
            Assert.AreEqual(errorError, validationException.Errors.First().ErrorMessage);
        }

        [Test]
        public void ThenValidRequestShouldNotThrowValidationExceptionWithValidationErrors()
        {
            Assert.That(async () => await _handler.Handle(_exampleValidRequest), Throws.Nothing);
        }

        //public class CommitmentAgreementComparer : IComparer<CommitmentAgreement>
        //{
        //    public int Compare(CommitmentAgreement x, CommitmentAgreement y)
        //    {
        //        new CompareLogic()
        //    }
        //}

        [Test]
        public async Task ThenCommitmentAgreementsReturnedByRepositoryShouldBeReturned()
        {
            const string commitmentReference = "ComRef", aleHash = "Lagunitas", legalEntityName = "legalEntity";

            var commitmentAgreements = new List<CommitmentAgreement>
            {
                new CommitmentAgreement
                {
                    Reference = commitmentReference,
                    AccountLegalEntityPublicHashedId = aleHash,
                    LegalEntityName = legalEntityName
                }
            };

            _mockCommitmentRespository.Setup(r => r.GetCommitmentAgreementsForProvider(1))
                .ReturnsAsync(TestHelper.Clone(commitmentAgreements));

            var response = await _handler.Handle(_exampleValidRequest);

            Assert.IsNotNull(response.Data);
            //todo: this
            //CollectionAssert.AreEqual(commitmentAgreements, response.Data);
        }
    }
}
