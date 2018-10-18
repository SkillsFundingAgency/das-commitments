using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetRelationship
{
    [TestFixture]
    public class WhenGettingRelationship
    {
        private Mock<IRelationshipRepository> _mockRelationshipRepository;
        private Mock<AbstractValidator<GetRelationshipRequest>> _validator;
        private Relationship _repositoryRecord;
        private GetRelationshipQueryHandler _handler;

        [SetUp]
        public void Arrange()
        {
            _mockRelationshipRepository = new Mock<IRelationshipRepository>();

            _repositoryRecord = new Relationship
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3",
                ProviderName = "Test Provider",
                Id = 101,
                LegalEntityName = "Test Legal Entity",
                Verified = false
            };

            _mockRelationshipRepository.Setup(
                x => x.GetRelationship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(_repositoryRecord);

            _validator = new Mock<AbstractValidator<GetRelationshipRequest>>();
            _validator.Setup(x => x.Validate(It.IsAny<GetRelationshipRequest>())).Returns(() => new ValidationResult());

            _handler = new GetRelationshipQueryHandler(_mockRelationshipRepository.Object, _validator.Object);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToGetRelationship()
        {
            //Act
            await _handler.Handle(new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3"
            });

            //Assert
            _mockRelationshipRepository.Verify(x=> x.GetRelationship(
                It.Is<long>(accountId => accountId == 1),
                It.Is<long>(providerId => providerId == 2),
                It.Is<string>(legalEntityId => legalEntityId == "L3")));
        }

        [Test]
        public async Task ThenValidationIsPerformed()
        {
            //Act
            await _handler.Handle(new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3"
            });

            //Assert
            _validator.Verify(x=> x.Validate(It.IsAny<GetRelationshipRequest>()));
        }


        [Test]
        public async Task ThenTheEntityIsMappedToTheModel()
        {
            //Act
            var result = await _handler.Handle(new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3"
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
            _mockRelationshipRepository.Setup(
                x => x.GetRelationship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync((Relationship)null);

            //Act
            var result = await _handler.Handle(new GetRelationshipRequest
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "L3"
            });

            //Assert
            Assert.IsNull(result.Data);
        }
    }
}
