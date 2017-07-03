using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetRelationshipByCommitment
{
    [TestFixture]
    public class WhenGettingRelationshipByCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<AbstractValidator<GetRelationshipByCommitmentRequest>> _validator;
        private Relationship _repositoryRecord;
        private Commitment _commitmentRecord;
        private GetRelationshipByCommitmentQueryHandler _handler;

        [SetUp]
        public void Arrange()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();

            _repositoryRecord = new Relationship
            {
                Id = 101,
                EmployerAccountId = 1,
                ProviderId = 1,
                LegalEntityId = "L3",
                LegalEntityName = "Test Legal Entity",
                ProviderName = "Test Provider Name",
                Verified = true
            };

            _commitmentRecord = new Commitment
            {
                Id = 1,
                EmployerAccountId = 1,
                ProviderId = 1,
                LegalEntityId = "L3",
            };

            _mockCommitmentRespository.Setup(
                x => x.GetRelationship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(_repositoryRecord);

            _mockCommitmentRespository.Setup(
                x => x.GetCommitmentById(It.IsAny<long>()))
                .ReturnsAsync(_commitmentRecord);

            _validator = new Mock<AbstractValidator<GetRelationshipByCommitmentRequest>>();
            _validator.Setup(x => x.Validate(It.IsAny<GetRelationshipByCommitmentRequest>())).Returns(() => new ValidationResult());

            _handler = new GetRelationshipByCommitmentQueryHandler(_mockCommitmentRespository.Object, _validator.Object);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToGetRelationship()
        {
            //Act
            await _handler.Handle(new GetRelationshipByCommitmentRequest
            {
                Caller = new Caller(1, CallerType.Provider),
                CommitmentId = 2
            });

            //Assert
            _mockCommitmentRespository.Verify(x => x.GetRelationship(
                It.Is<long>(accountId => accountId == 1),
                It.Is<long>(providerId => providerId == 1),
                It.Is<string>(legalEntityId => legalEntityId == "L3")), Times.Once);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToGetCommitment()
        {
            //Act
            await _handler.Handle(new GetRelationshipByCommitmentRequest
            {
                Caller = new Caller(1, CallerType.Provider),
                CommitmentId = 2
            });

            //Assert
            _mockCommitmentRespository.Verify(x => x.GetCommitmentById(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task ThenValidationIsPerformed()
        {
            //Act
            await _handler.Handle(new GetRelationshipByCommitmentRequest
            {
                Caller = new Caller(1, CallerType.Provider),
                CommitmentId = 2
            });

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<GetRelationshipByCommitmentRequest>()), Times.Once);
        }


        [Test]
        public async Task ThenTheEntityIsMappedToTheModel()
        {
            //Act
            var result = await _handler.Handle(new GetRelationshipByCommitmentRequest
            {
                Caller = new Caller(1, CallerType.Provider),
                CommitmentId = 2
            });

            //Assert
            Assert.AreEqual(_repositoryRecord.LegalEntityId, result.Data.LegalEntityId);
            Assert.AreEqual(_repositoryRecord.EmployerAccountId, result.Data.EmployerAccountId);
            Assert.AreEqual(_repositoryRecord.LegalEntityName, result.Data.LegalEntityName);
            Assert.AreEqual(_repositoryRecord.ProviderName, result.Data.ProviderName);
            Assert.AreEqual(_repositoryRecord.Verified, result.Data.Verified);
            Assert.AreEqual(_repositoryRecord.Id, result.Data.Id);
            Assert.AreEqual(_repositoryRecord.ProviderId, result.Data.ProviderId);
        }

        [Test]
        public async Task ThenIfTheRelationshipIsNotFoundThenTheModelWillBeNull()
        {
            //Arrange
            _mockCommitmentRespository.Setup(
                x => x.GetRelationship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(null);

            //Act
            var result = await _handler.Handle(new GetRelationshipByCommitmentRequest
            {
                Caller = new Caller(1, CallerType.Provider),
                CommitmentId = 2
            });

            //Assert
            Assert.IsNull(result.Data);
        }
    }
}
