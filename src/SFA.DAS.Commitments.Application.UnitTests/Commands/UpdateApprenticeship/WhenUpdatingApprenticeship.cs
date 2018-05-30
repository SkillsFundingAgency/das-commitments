using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Messaging.Interfaces;

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
        private IEnumerable<HistoryItem> _historyResult;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockValidator = new Mock<AbstractValidator<UpdateApprenticeshipCommand>>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockApprenticeshipUpdateRules = new Mock<IApprenticeshipUpdateRules>();

            _mockHistoryRepository.Setup(x => x.InsertHistory(It.IsAny<IEnumerable<HistoryItem>>()))
                .Callback((object o) => { _historyResult = o as IEnumerable<HistoryItem>; })
                .Returns(() => Task.CompletedTask);

            _handler = new UpdateApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object, 
                _mockApprenticeshipRepository.Object, 
                _mockValidator.Object,
                _mockApprenticeshipUpdateRules.Object, 
                _mockApprenticeshipEvents.Object, 
                Mock.Of<ICommitmentsLogger>(),
                _mockHistoryRepository.Object,
                Mock.Of<IMessagePublisher>());
            
            _mockValidator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipCommand>())).Returns(new ValidationResult());

            var fixture = new Fixture();
            var populatedApprenticeship = fixture.Build<Apprenticeship>().Create();

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
                ProviderId = _exampleValidRequest.Caller.Id,
                Apprenticeships =
                {
                    new Apprenticeship
                    {
                        Id = _exampleValidRequest.ApprenticeshipId,
                        PaymentStatus = PaymentStatus.PendingApproval
                    }
                }
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
                ProviderId = _exampleValidRequest.Caller.Id++,
                Apprenticeships =
                {
                    new Apprenticeship
                    {
                        Id = _exampleValidRequest.ApprenticeshipId
                    }
                }
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
                                        new Apprenticeship { Id = 1, CommitmentId = 123L, AgreementStatus = AgreementStatus.BothAgreed, PaymentStatus = PaymentStatus.PendingApproval },
                                        new Apprenticeship { Id = 2, CommitmentId = 123L, AgreementStatus = AgreementStatus.EmployerAgreed, PaymentStatus = PaymentStatus.PendingApproval },
                                        new Apprenticeship { Id = 3, CommitmentId = 123L, AgreementStatus = AgreementStatus.ProviderAgreed, PaymentStatus = PaymentStatus.PendingApproval },
                                        new Apprenticeship { Id = 4, CommitmentId = 123L, AgreementStatus = AgreementStatus.NotAgreed, PaymentStatus = PaymentStatus.PendingApproval }
                                    }
            };

            _mockApprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(
                    new Apprenticeship
                        {
                            Id = 1,
                            AgreementStatus = AgreementStatus.NotAgreed,
                            PaymentStatus = PaymentStatus.Active
                        });

            _mockCommitmentRespository.Setup(m => m.GetCommitmentById(It.IsAny<long>())).Returns(Task.Run(() => c));
            _exampleValidRequest.ApprenticeshipId = 1;
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRepository.Verify(x =>
                x.UpdateApprenticeshipStatus(123, It.IsAny<long>(), AgreementStatus.NotAgreed), Times.Exactly(2));
        }

        [Test]
        public async Task ThenHistoryRecordsAreCreated()
        {
            var testApprenticeship = new Apprenticeship
            {
                Id = _exampleValidRequest.ApprenticeshipId,
                PaymentStatus = PaymentStatus.PendingApproval,
                CommitmentId = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            };

            var testCommitment = new Commitment
            {
                ProviderId = _exampleValidRequest.Caller.Id,
                Id = _exampleValidRequest.CommitmentId,
                Apprenticeships = { testApprenticeship }
            };

            var expectedOriginalCommitmentState = JsonConvert.SerializeObject(testCommitment);
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            _mockApprenticeshipRepository.Setup(x => x.GetApprenticeship(_exampleValidRequest.ApprenticeshipId)).ReturnsAsync(testApprenticeship);
            var expectedOriginalApprenticeshipState = JsonConvert.SerializeObject(testApprenticeship);

            await _handler.Handle(_exampleValidRequest);

            var expectedNewApprenticeshipState = JsonConvert.SerializeObject(testApprenticeship);

            Assert.AreEqual(2, _historyResult.Count());

            Assert.AreEqual(1, _historyResult.Count(item =>
                item.ChangeType == CommitmentChangeType.EditedApprenticeship.ToString()
                && item.CommitmentId == testCommitment.Id
                && item.ApprenticeshipId == null
                && item.OriginalState == expectedOriginalCommitmentState
                && item.UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString()
                && item.UpdatedState == expectedOriginalCommitmentState
                && item.UserId == _exampleValidRequest.UserId
                && item.ProviderId == testApprenticeship.ProviderId
                && item.EmployerAccountId == testApprenticeship.EmployerAccountId
                && item.UpdatedByName == _exampleValidRequest.UserName
            ));

            Assert.AreEqual(1, _historyResult.Count(item =>
                item.ChangeType == ApprenticeshipChangeType.Updated.ToString()
                && item.CommitmentId == null
                && item.ApprenticeshipId == testApprenticeship.Id
                && item.OriginalState == expectedOriginalApprenticeshipState
                && item.UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString()
                && item.UpdatedState == expectedNewApprenticeshipState
                && item.UserId == _exampleValidRequest.UserId
                && item.ProviderId == testApprenticeship.ProviderId
                && item.EmployerAccountId == testApprenticeship.EmployerAccountId
                && item.UpdatedByName == _exampleValidRequest.UserName
            ));
        }

        [Test]
        public async Task ThenIfNoChangesWereMadeThenTheAgreementStatusRemainsUnchanged()
        {
            //Arrange
            var testApprenticeship = new Apprenticeship
            {
                Id = _exampleValidRequest.ApprenticeshipId,
                AgreementStatus = AgreementStatus.EmployerAgreed
            };

            var testCommitment = new Commitment
            {
                ProviderId = _exampleValidRequest.Caller.Id,
                Id = _exampleValidRequest.CommitmentId,
                Apprenticeships = { testApprenticeship }
            };
            
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

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
