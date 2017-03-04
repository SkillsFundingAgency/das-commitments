using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.VerifyRelationship;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.VerifyRelationship
{
    [TestFixture]
    public class WhenVerifyingRelationship
    {
        private VerifyRelationshipCommandHandler _handler;

        private Mock<VerifyRelationshipValidator> _validator;
        private Mock<ICommitmentRepository> _repository;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<VerifyRelationshipValidator>();

            _repository = new Mock<ICommitmentRepository>();
            _repository.Setup(x => x.VerifyRelationship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(()=> Task.FromResult(new Unit()));

            _handler = new VerifyRelationshipCommandHandler(_repository.Object, _validator.Object, Mock.Of<ICommitmentsLogger>());            
        }

        [Test]
        public async Task ThenIfTheRequestIsValidThenTheRepositoryIsCalled()
        {
            //Arrange
            var request = new VerifyRelationshipCommand
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3",
                Verified = true
            };
            _validator.Setup(x => x.Validate(It.IsAny<VerifyRelationshipCommand>())).Returns(new ValidationResult());


            //Act
            await _handler.Handle(request);

            //Assert
            _repository.Verify(x => x.VerifyRelationship(
                It.Is<long>(y=> y == 1),
                It.Is<long>(y => y == 2),
                It.Is<string>(y => y == "L3"),
                It.Is<bool>(y => y == true)),
                Times.Once);
        }

        [Test]
        public async Task ThenIfTheRequestIsNotValidThenTheRepositoryIsNotCalled()
        {
            //Arrange
            var request = new VerifyRelationshipCommand();
            _validator.Setup(x => x.Validate(It.IsAny<VerifyRelationshipCommand>()))
                .Returns(new ValidationResult
                {
                    Errors = { new ValidationFailure("Test", "Test Error") }
                });

            //Act & Assert
            Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(request)
            );
            
            _repository.Verify(x => x.VerifyRelationship(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<bool>()),
                Times.Never);
        }
    }
}
