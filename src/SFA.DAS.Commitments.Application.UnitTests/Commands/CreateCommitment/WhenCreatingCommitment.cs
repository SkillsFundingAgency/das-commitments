using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentValidation;

using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;
using SFA.DAS.HashingService;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateCommitment
{
    [TestFixture]
    public sealed class WhenCreatingCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private CreateCommitmentCommandHandler _handler;
        private CreateCommitmentCommand _exampleValidRequest;
        private Mock<IHashingService> _mockHashingService;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IMessagePublisher> _messagePublisher;

        private Commitment _populatedCommitment;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockHashingService = new Mock<IHashingService>();
			var commandValidator = new CreateCommitmentValidator();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _messagePublisher = new Mock<IMessagePublisher>();

           _handler = new CreateCommitmentCommandHandler(_mockCommitmentRespository.Object,
                _mockHashingService.Object,
                commandValidator,
                Mock.Of<ICommitmentsLogger>(),
                _mockHistoryRepository.Object,
                _messagePublisher.Object);

            var fixture = new Fixture();
            fixture.Customize<Apprenticeship>(ob => ob
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
            _populatedCommitment = fixture.Build<Commitment>().Create();
            _populatedCommitment.Apprenticeships = new List<Apprenticeship>();

            _exampleValidRequest = new CreateCommitmentCommand
                                       {
                                           Commitment = _populatedCommitment,
                                           Caller = new Caller(1L, CallerType.Employer),
                                           UserId = "UserId"
                                       };
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
            //Arrange
            Commitment argument = null;
            _mockCommitmentRespository.Setup(
                    x => x.Create(It.IsAny<Commitment>()))
                .ReturnsAsync(4)
                .Callback<Commitment>((commitment) => argument = commitment);

            //Act
            await _handler.Handle(TestHelper.Clone(_exampleValidRequest));

            //Assert
            argument.Should().NotBeNull();
            AssertMappingIsCorrect(argument);
        }

        [Test]
        public async Task ThenShouldReturnTheCommitmentIdReturnedFromRepository()
        {
            const long expectedCommitmentId = 45;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Commitment>())).ReturnsAsync(expectedCommitmentId);

            var commitmentId = await _handler.Handle(_exampleValidRequest);

            commitmentId.Should().Be(expectedCommitmentId);
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Commitment.Reference = null; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
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
                            m => m.Author == _exampleValidRequest.Commitment.LastUpdatedByEmployerName && m.CreatedBy == _exampleValidRequest.Caller.CallerType && m.Text == _exampleValidRequest.Message)),
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
                                y.First().ChangeType == CommitmentChangeType.Created.ToString() &&
                                y.First().CommitmentId == expectedCommitmentId &&
                                y.First().ApprenticeshipId == null &&
                                y.First().OriginalState == null &&
                                y.First().UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString() &&
                                y.First().UpdatedState != null &&
                                y.First().UserId == _exampleValidRequest.UserId &&
                                y.First().ProviderId == _populatedCommitment.ProviderId &&
                                y.First().EmployerAccountId == _populatedCommitment.EmployerAccountId &&
                                y.First().UpdatedByName == _exampleValidRequest.Commitment.LastUpdatedByEmployerName)), Times.Once);
        }

        [Test]
        public async Task ThenTheCohortEventIsCreated()
        {
            const long expectedCommitmentId = 45;
            _mockCommitmentRespository.Setup(x => x.Create(It.IsAny<Commitment>())).ReturnsAsync(expectedCommitmentId);

            await _handler.Handle(_exampleValidRequest);

            _messagePublisher.Verify(
                x =>
                    x.PublishAsync(
                        It.Is<CohortCreated>(
                            m => m.AccountId == _exampleValidRequest.Commitment.EmployerAccountId && m.ProviderId == _exampleValidRequest.Commitment.ProviderId.Value && m.CommitmentId == expectedCommitmentId)), Times.Once);
        }

        private void AssertMappingIsCorrect(Commitment argument)
        {
            argument.Reference.Should().Be(_exampleValidRequest.Commitment.Reference);
            argument.EmployerAccountId.Should().Be(_exampleValidRequest.Commitment.EmployerAccountId);
            argument.LegalEntityId.Should().Be(_exampleValidRequest.Commitment.LegalEntityId);
            argument.LegalEntityAddress.Should().Be(_exampleValidRequest.Commitment.LegalEntityAddress);
            argument.LegalEntityOrganisationType.Should().Be(_exampleValidRequest.Commitment.LegalEntityOrganisationType);
            argument.AccountLegalEntityPublicHashedId.Should().Be(_exampleValidRequest.Commitment.AccountLegalEntityPublicHashedId);
            argument.ProviderId.Should().Be(_exampleValidRequest.Commitment.ProviderId);
            argument.CommitmentStatus.Should().Be(CommitmentStatus.New);
            argument.LastAction.Should().Be(LastAction.None);
            argument.LastUpdatedByEmployerName.Should().Be(_exampleValidRequest.Commitment.LastUpdatedByEmployerName);
            argument.LastUpdatedByEmployerEmail.Should().Be(_exampleValidRequest.Commitment.LastUpdatedByEmployerEmail);
            argument.Apprenticeships.Should().BeEmpty();
        }
    }
}
