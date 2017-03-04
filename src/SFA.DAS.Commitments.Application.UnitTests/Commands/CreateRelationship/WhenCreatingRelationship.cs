using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateRelationship;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateRelationship
{
    [TestFixture]
    public class WhenCreatingRelationship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<AbstractValidator<CreateRelationshipCommand>> _validator;
        private CreateRelationshipCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<CreateRelationshipCommand>>();
           
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();

            _mockCommitmentRespository.Setup(
                x => x.CreateRelationship(It.IsAny<Domain.Entities.Relationship>()))
                    .ReturnsAsync(1);

            _handler = new CreateRelationshipCommandHandler(_mockCommitmentRespository.Object,
                _validator.Object, Mock.Of<ICommitmentsLogger>());
        }


        public async Task ThenIfRequestIsValidThenTheRepositoryIsCalled()
        {
            //Arrange
            _validator.Setup(x => x.Validate(It.IsAny<CreateRelationshipCommand>()))
                .Returns(() => new ValidationResult());

            //Act
            await _handler.Handle(new CreateRelationshipCommand
            {
                Relationship = new Relationship()
            });

            //Assert
            _mockCommitmentRespository.Verify(
                x => x.CreateRelationship(It.IsAny<Domain.Entities.Relationship>()), Times.Once);
        }

        public async Task ThenIfRequestIsNotValidThenTheRepositoryIsNotCalled()
        {
            //Arrange
            _validator.Setup(x => x.Validate(It.IsAny<CreateRelationshipCommand>()))
                .Returns(() => new ValidationResult
                {
                    Errors = { new ValidationFailure("Test", "Test Error")}
                });

            //Act
            await _handler.Handle(new CreateRelationshipCommand
            {
                Relationship = new Relationship()
            });

            //Assert
            _mockCommitmentRespository.Verify(
                x => x.CreateRelationship(It.IsAny<Domain.Entities.Relationship>()), Times.Never);
        }
    }
}
