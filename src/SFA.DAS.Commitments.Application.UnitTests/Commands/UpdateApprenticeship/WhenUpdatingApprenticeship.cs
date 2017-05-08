using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeship
{
    [TestFixture]
    public sealed class WhenUpdatingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
        private UpdateApprenticeshipCommandHandler _handler;
        private UpdateApprenticeshipCommand _exampleValidRequest;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<AbstractValidator<UpdateApprenticeshipCommand>> _mockValidator;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IApprenticeshipUpdateRules> _mockApprenticeshipUpdateRules;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockValidator = new Mock<AbstractValidator<UpdateApprenticeshipCommand>>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockApprenticeshipUpdateRules = new Mock<IApprenticeshipUpdateRules>();

            _handler = new UpdateApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object, 
                _mockApprenticeshipRepository.Object, 
                _mockValidator.Object,
                _mockApprenticeshipUpdateRules.Object, 
                _mockApprenticeshipEvents.Object, 
                Mock.Of<ICommitmentsLogger>(),
                _mockHistoryRepository.Object);
            
            _mockValidator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipCommand>())).Returns(new ValidationResult());

            var fixture = new Fixture();
            var populatedApprenticeship = fixture.Build<Api.Types.Apprenticeship.Apprenticeship>().Create();

            _exampleValidRequest = new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                ApprenticeshipId = populatedApprenticeship.Id,
                Apprenticeship = populatedApprenticeship,
                UserName = "Bob"
            };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });

            _mockApprenticeshipRepository.Setup(x => x.GetApprenticeship(_exampleValidRequest.ApprenticeshipId)).ReturnsAsync(new Apprenticeship
            {
                Id = _exampleValidRequest.ApprenticeshipId,
                PaymentStatus = PaymentStatus.PendingApproval
            });

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRepository.Verify(x => x.UpdateApprenticeship(It.IsAny<Apprenticeship>(), It.Is<Caller>(m => m.CallerType == CallerType.Provider)));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            var validationFailureResult = new ValidationResult(new List<ValidationFailure>() { new ValidationFailure("test", "error text") });
            _mockValidator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipCommand>())).Returns(validationFailureResult);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id++
            });

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public async Task ThenItShouldUpdatedTheAgreementStatusForAllApprenticeshipsOnTheSameCommitment()
        {
            var c = new Commitment
            {
                Id = 123L,
                EditStatus = EditStatus.ProviderOnly,
                ProviderId = 111L,
                EmployerAccountId = 5L,
                CommitmentStatus = CommitmentStatus.Active,
                Apprenticeships =
                                new List<Apprenticeship>
                                    {
                                        new Apprenticeship { Id = 1, AgreementStatus = AgreementStatus.BothAgreed, PaymentStatus = PaymentStatus.PendingApproval },
                                        new Apprenticeship { Id = 2, AgreementStatus = AgreementStatus.EmployerAgreed, PaymentStatus = PaymentStatus.PendingApproval },
                                        new Apprenticeship { Id = 3, AgreementStatus = AgreementStatus.ProviderAgreed, PaymentStatus = PaymentStatus.PendingApproval },
                                        new Apprenticeship { Id = 4, AgreementStatus = AgreementStatus.NotAgreed, PaymentStatus = PaymentStatus.PendingApproval }
                                    }
            };

            _mockApprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(
                    new Apprenticeship
                        {
                            Id = 99,
                            AgreementStatus = AgreementStatus.NotAgreed,
                            PaymentStatus = PaymentStatus.Active
                        });

            _mockCommitmentRespository.Setup(m => m.GetCommitmentById(It.IsAny<long>())).Returns(Task.Run(() => c));
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRepository.Verify(x =>
                x.UpdateApprenticeshipStatus(123, It.IsAny<long>(), AgreementStatus.NotAgreed), Times.Exactly(3));
        }

        [Test]
        public async Task ThenHistoryRecordsAreCreated()
        {
            var testCommitment = new Commitment
            {
                ProviderId = _exampleValidRequest.Caller.Id,
                Id = _exampleValidRequest.CommitmentId
            };
            var expectedOriginalCommitmentState = JsonConvert.SerializeObject(testCommitment);
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            var testApprenticeship = new Apprenticeship
            {
                Id = _exampleValidRequest.ApprenticeshipId,
                PaymentStatus = PaymentStatus.PendingApproval
            };
            _mockApprenticeshipRepository.Setup(x => x.GetApprenticeship(_exampleValidRequest.ApprenticeshipId)).ReturnsAsync(testApprenticeship);
            var expectedOriginalApprenticeshipState = JsonConvert.SerializeObject(testApprenticeship);

            await _handler.Handle(_exampleValidRequest);

            var expectedNewApprenticeshipState = JsonConvert.SerializeObject(testApprenticeship);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().EntityId == testCommitment.Id &&
                                y.First().ChangeType == CommitmentChangeType.EditedApprenticeship.ToString() &&
                                y.First().EntityType == "Commitment" &&
                                y.First().OriginalState == expectedOriginalCommitmentState &&
                                y.First().UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == expectedOriginalCommitmentState &&
                                y.First().UserId == _exampleValidRequest.UserId &&
                                y.First().UpdatedByName == _exampleValidRequest.UserName)), Times.Once);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.Last().EntityId == testApprenticeship.Id &&
                                y.Last().ChangeType == ApprenticeshipChangeType.Updated.ToString() &&
                                y.Last().EntityType == "Apprenticeship" &&
                                y.Last().OriginalState == expectedOriginalApprenticeshipState &&
                                y.Last().UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString() &&
                                y.Last().UpdatedState == expectedNewApprenticeshipState &&
                                y.Last().UserId == _exampleValidRequest.UserId &&
                                y.Last().UpdatedByName == _exampleValidRequest.UserName)), Times.Once);
        }

        [Test]
        public async Task ThenIfNoChangesWereMadeThenTheAgreementStatusRemainsUnchanged()
        {
            //Arrange
            var testCommitment = new Commitment
            {
                ProviderId = _exampleValidRequest.Caller.Id,
                Id = _exampleValidRequest.CommitmentId
            };
            
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            var testApprenticeship = new Apprenticeship
            {
                Id = _exampleValidRequest.ApprenticeshipId,
                AgreementStatus = AgreementStatus.EmployerAgreed
            };
            _mockApprenticeshipRepository.Setup(x => x.GetApprenticeship(_exampleValidRequest.ApprenticeshipId)).ReturnsAsync(testApprenticeship);

            _mockApprenticeshipUpdateRules.Setup(x => x.DetermineNewAgreementStatus(
                    It.IsAny<AgreementStatus>(),
                    It.IsAny<CallerType>(),
                    It.IsAny<bool>()))
                .Returns(() => AgreementStatus.EmployerAgreed);

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockApprenticeshipUpdateRules.Verify(x => x.DetermineNewAgreementStatus(
                It.Is<AgreementStatus>(s => s == AgreementStatus.EmployerAgreed),
                It.IsAny<CallerType>(),
                It.IsAny<bool>()),
                Times.Once);
        }

    }
}
