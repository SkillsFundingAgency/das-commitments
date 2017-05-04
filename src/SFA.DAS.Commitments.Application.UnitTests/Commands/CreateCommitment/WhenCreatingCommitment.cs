using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Commands.CreateRelationship;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;
using Relationship = SFA.DAS.Commitments.Api.Types.Relationship;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateCommitment
{
    [TestFixture]
    public sealed class WhenCreatingCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private CreateCommitmentCommandHandler _handler;
        private CreateCommitmentCommand _exampleValidRequest;
        private Mock<IHashingService> _mockHashingService;
        private Mock<IMediator> _mockMediator;
        private Mock<IHistoryRepository> _mockHistoryRepository;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockHashingService = new Mock<IHashingService>();
			var commandValidator = new CreateCommitmentValidator();
			_mockMediator = new Mock<IMediator>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _handler = new CreateCommitmentCommandHandler(_mockCommitmentRespository.Object, 
                _mockHashingService.Object,
                commandValidator,
                Mock.Of<ICommitmentsLogger>(),
                _mockMediator.Object,
                _mockHistoryRepository.Object);

            Fixture fixture = new Fixture();
            fixture.Customize<Api.Types.Apprenticeship.Apprenticeship>(ob => ob
                .With(x => x.ULN, ApprenticeshipTestDataHelper.CreateValidULN())
                .With(x => x.NINumber, ApprenticeshipTestDataHelper.CreateValidNino())
                .With(x => x.FirstName, "First name")
                .With(x => x.FirstName, "Last name")
                .With(x => x.ProviderRef, "Provider ref")
                .With(x => x.EmployerRef, null)
                .With(x => x.StartDate, DateTime.Now.AddYears(5))
                .With(x => x.EndDate, DateTime.Now.AddYears(7))
                .With(x => x.DateOfBirth, DateTime.Now.AddYears(-16))
                .With(x => x.TrainingCode, string.Empty)
                .With(x => x.TrainingName, string.Empty)
            );
            var populatedCommitment = fixture.Build<Api.Types.Commitment.Commitment>().Create();
            _exampleValidRequest = new CreateCommitmentCommand { Commitment = populatedCommitment, CallerType = CallerType.Employer, UserId = "UserId"};

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetRelationshipRequest>()))
               .ReturnsAsync(new GetRelationshipResponse
               {
                   Data = new Api.Types.Relationship()
               });

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateRelationshipCommand>()))
                .ReturnsAsync(new Unit());
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.Create(It.IsAny<Domain.Entities.Commitment>()));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryWithCommitmentMappedFromRequest()
        {
            Domain.Entities.Commitment argument = null;
            _mockCommitmentRespository.Setup(
                    x => x.Create(It.IsAny<Domain.Entities.Commitment>()))
                .ReturnsAsync(4)
                .Callback<Domain.Entities.Commitment>(
                    ((commitment) => argument = commitment));

            await _handler.Handle(_exampleValidRequest);

            argument.Should().NotBeNull();
            AssertMappingIsCorrect(argument);
        }

        [Test]
        public async Task ThenShouldReturnTheCommitmentIdReturnedFromRepository()
        {
            const long ExpectedCommitmentId = 45;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Domain.Entities.Commitment>())).ReturnsAsync(ExpectedCommitmentId);

            var commitmentId = await _handler.Handle(_exampleValidRequest);

            commitmentId.Should().Be(ExpectedCommitmentId);
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Commitment.Reference = null; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenShouldGetProviderLegalEntityRelationship()
        {
            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockMediator.Verify(x=> x.SendAsync(It.Is<GetRelationshipRequest>
                (r=> r.EmployerAccountId == _exampleValidRequest.Commitment.EmployerAccountId
                && r.ProviderId == _exampleValidRequest.Commitment.ProviderId.Value
                && r.LegalEntityId == _exampleValidRequest.Commitment.LegalEntityId
                )));
        }

        [Test]
        public async Task ThenIfProviderLegalEntityRelationshipDoesNotExistThenShouldCreateIt()
        {
            //Arrange
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetRelationshipRequest>()))
               .ReturnsAsync(new GetRelationshipResponse
               {
                   Data = null
               });

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(It.Is<CreateRelationshipCommand>(
                r=> r.Relationship != null
                && r.Relationship.ProviderId == _exampleValidRequest.Commitment.ProviderId.Value
                && r.Relationship.ProviderName == _exampleValidRequest.Commitment.ProviderName
                && r.Relationship.EmployerAccountId == _exampleValidRequest.Commitment.EmployerAccountId
                && r.Relationship.LegalEntityId == _exampleValidRequest.Commitment.LegalEntityId
                && r.Relationship.LegalEntityName == _exampleValidRequest.Commitment.LegalEntityName
                )), Times.Once);
        }

        [Test]
        public async Task ThenIfProviderLegalEntityRelationshipExistsThenShouldNotCreateAnother()
        {
            //Arrange
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetRelationshipRequest>()))
               .ReturnsAsync(new GetRelationshipResponse
               {
                   Data = new Relationship()
               });

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockMediator.Verify(x=> x.SendAsync(It.IsAny<CreateRelationshipCommand>()), Times.Never);
        }

        [Test]
        public async Task ThenIfNoMessageIsProvidedThenAMessageIsNotSaved()
        {
            _exampleValidRequest.Message = null;

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockCommitmentRespository.Verify(x => x.SaveMessage(It.IsAny<long>(), It.IsAny<Message>()), Times.Never);
        }

        [Test]
        public async Task ThenIfAMessageIsProvidedThenTheMessageIsSaved()
        {
            //Arange
            const long expectedCommitmentId = 45;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Commitment>())).ReturnsAsync(expectedCommitmentId);
            _exampleValidRequest.Message = "New Message";

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockCommitmentRespository.Verify(
                x =>
                    x.SaveMessage(expectedCommitmentId,
                        It.Is<Message>(
                            m => m.Author == _exampleValidRequest.Commitment.EmployerLastUpdateInfo.Name && m.CreatedBy == _exampleValidRequest.CallerType && m.Text == _exampleValidRequest.Message)),
                Times.Once);
        }

        [Test]
        public async Task ThenAHistoryRecordIsCreated()
        {
            const long expectedCommitmentId = 45;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Commitment>())).ReturnsAsync(expectedCommitmentId);

            await _handler.Handle(_exampleValidRequest);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().EntityId == expectedCommitmentId && 
                                y.First().ChangeType == CommitmentChangeType.Created.ToString() && 
                                y.First().EntityType == "Commitment" && 
                                y.First().OriginalState == null &&
                                y.First().UpdatedByRole == _exampleValidRequest.CallerType.ToString() &&
                                y.First().UpdatedState != null &&
                                y.First().UserId == _exampleValidRequest.UserId &&
                                y.First().UpdatedByName == _exampleValidRequest.Commitment.EmployerLastUpdateInfo.Name)), Times.Once);
        }

        private void AssertMappingIsCorrect(Domain.Entities.Commitment argument)
        {
            argument.Reference.Should().Be(_exampleValidRequest.Commitment.Reference);
            argument.EmployerAccountId.Should().Be(_exampleValidRequest.Commitment.EmployerAccountId);
            argument.LegalEntityId.Should().Be(_exampleValidRequest.Commitment.LegalEntityId);
            argument.LegalEntityAddress.Should().Be(_exampleValidRequest.Commitment.LegalEntityAddress);
            argument.LegalEntityOrganisationType.Should().Be((Domain.Entities.OrganisationType)_exampleValidRequest.Commitment.LegalEntityOrganisationType);
            argument.ProviderId.Should().Be(_exampleValidRequest.Commitment.ProviderId);
            argument.CommitmentStatus.Should().Be(CommitmentStatus.New);
            argument.LastAction.Should().Be(LastAction.None);
            argument.LastUpdatedByEmployerName.Should().Be(_exampleValidRequest.Commitment.EmployerLastUpdateInfo.Name);
            argument.LastUpdatedByEmployerEmail.Should().Be(_exampleValidRequest.Commitment.EmployerLastUpdateInfo.EmailAddress);
            argument.Apprenticeships.Should().BeEmpty();
        }
    }
}
