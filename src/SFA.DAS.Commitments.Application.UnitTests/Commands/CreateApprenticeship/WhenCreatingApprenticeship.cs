using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;
using TrainingType = SFA.DAS.Commitments.Domain.Entities.TrainingType;
using SFA.DAS.Learners.Validators;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeship
{
    [TestFixture]
    public sealed class WhenCreatingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
        private CreateApprenticeshipCommandHandler _handler;
        private CreateApprenticeshipCommand _exampleValidRequest;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IUlnValidator> _mockUlnValidator;
        private Mock<IAcademicYearValidator> _mockAcademicYearValidator;

        private long expectedApprenticeshipId = 12;

        private readonly long _providerId = 10012;
        private readonly long _employerAccountId = 10013;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockUlnValidator = new Mock<IUlnValidator>();
            _mockAcademicYearValidator = new Mock<IAcademicYearValidator>();

            var validator = new CreateApprenticeshipValidator(new ApprenticeshipValidator(new StubCurrentDateTime(), _mockUlnValidator.Object, _mockAcademicYearValidator.Object));
            _handler = new CreateApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRepository.Object,
                validator, 
                _mockApprenticeshipEvents.Object, 
                Mock.Of<ICommitmentsLogger>(),
                _mockHistoryRepository.Object,
                Mock.Of<IMessagePublisher>());

            var fixture = new Fixture();
            var populatedApprenticeship = fixture.Build<Apprenticeship>()
                .With(x => x.ULN, "1234567890")
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
                .Create();

            _mockApprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Domain.Entities.Apprenticeship {Id = expectedApprenticeshipId, ProviderId = _providerId, EmployerAccountId = _employerAccountId });

            _exampleValidRequest = new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                Apprenticeship = populatedApprenticeship,
                UserId = "ABBA123"
            };
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToCreateApprenticeship()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRepository.Verify(x => 
                x.CreateApprenticeship(It.IsAny<Domain.Entities.Apprenticeship>()));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatusOfTheApprenticeshipsToNotAgreed()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id,
                Apprenticeships = new System.Collections.Generic.List<Domain.Entities.Apprenticeship>
                {
                    new Domain.Entities.Apprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed },
                    new Domain.Entities.Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed },
                    new Domain.Entities.Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed },
                }
            });

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRepository.Verify(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Domain.Entities.AgreementStatus>()), Times.Exactly(2));
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryWithApprenticeshipMappedFromRequest()
        {
            Domain.Entities.Apprenticeship argument = null;
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });

            _mockApprenticeshipRepository
                .Setup(x => x.CreateApprenticeship(It.IsAny<Domain.Entities.Apprenticeship>()))
                .ReturnsAsync(_exampleValidRequest.Apprenticeship.Id)
                .Callback<Domain.Entities.Apprenticeship>((x) => argument = x);

            await _handler.Handle(_exampleValidRequest);

            argument.Should().NotBeNull();
            AssertMappingIsCorrect(argument);
        }

        [Test]
        public async Task ThenShouldReturnTheApprenticeshipIdReturnedFromRepository()
        {
            const long expectedApprenticeshipId = 88;

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id
            });
            _mockApprenticeshipRepository.Setup(x => x.CreateApprenticeship(It.IsAny<Domain.Entities.Apprenticeship>())).ReturnsAsync(expectedApprenticeshipId);

            var commitmentId = await _handler.Handle(_exampleValidRequest);

            commitmentId.Should().Be(expectedApprenticeshipId);
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Apprenticeship = null; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [TestCase(EditStatus.ProviderOnly, CallerType.Provider)]
        [TestCase(EditStatus.Both, CallerType.Provider)]
        [TestCase(EditStatus.EmployerOnly, CallerType.Employer)]
        [TestCase(EditStatus.Both, CallerType.Employer)]
        public void ThenWhenEditStatusIsCorrectNoExceptionIsThrown(EditStatus editStatus, CallerType callerType)
        {
            var c = new Commitment { EditStatus = editStatus, ProviderId = 5L, EmployerAccountId = 5L };
            _exampleValidRequest.Caller = new Caller { Id = 5L, CallerType = callerType };
            _mockCommitmentRespository.Setup(m => m.GetCommitmentById(It.IsAny<long>())).Returns(Task.Run(() => c));
            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldNotThrow<UnauthorizedException>();
        }

        [TestCase(EditStatus.EmployerOnly, CallerType.Provider)]
        [TestCase(EditStatus.Neither, CallerType.Provider)]
        [TestCase(EditStatus.ProviderOnly, CallerType.Employer)]
        [TestCase(EditStatus.Neither, CallerType.Employer)]
        public void ThenWhenEditStatusIsIncorrectAnInvalidRequestExceptionIsThrown(EditStatus editStatus, CallerType callerType)
        {
            var c = new Commitment { EditStatus = editStatus, ProviderId = 5L, EmployerAccountId = 5L, CommitmentStatus = CommitmentStatus.Active};
            _exampleValidRequest.Caller = new Caller { Id = 5L, CallerType = callerType };
            _mockCommitmentRespository.Setup(m => m.GetCommitmentById(It.IsAny<long>())).Returns(Task.Run(() => c));
            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [TestCase(CommitmentStatus.Active)]
        [TestCase(CommitmentStatus.New)]
        public void ThenWhenCommitmentStatusIsCorrectNoExceptionIsThrown(CommitmentStatus commitmentStatus)
        {
            var c = new Commitment { EditStatus = EditStatus.EmployerOnly, ProviderId = 5L, EmployerAccountId = 5L, CommitmentStatus = commitmentStatus };
            _exampleValidRequest.Caller = new Caller { Id = 5L, CallerType = CallerType.Employer };
            _mockCommitmentRespository.Setup(m => m.GetCommitmentById(It.IsAny<long>())).Returns(Task.Run(() => c));
            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldNotThrow<InvalidOperationException>();
        }

        [TestCase(CommitmentStatus.Deleted)]
        public void ThenWhenCommitmentStatusIsIncorrectAnInvalidRequestExceptionIsThrown(CommitmentStatus commitmentStatus)
        {
            var c = new Commitment { EditStatus = EditStatus.EmployerOnly, ProviderId = 5L, EmployerAccountId = 5L, CommitmentStatus = commitmentStatus };
            _exampleValidRequest.Caller = new Caller { Id = 5L, CallerType = CallerType.Employer };
            _mockCommitmentRespository.Setup(m => m.GetCommitmentById(It.IsAny<long>())).Returns(Task.Run(() => c));
            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidOperationException>();
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
                                new List<Domain.Entities.Apprenticeship>
                                    {
                                        new Domain.Entities.Apprenticeship { Id = 1, AgreementStatus = AgreementStatus.BothAgreed},
                                        new Domain.Entities.Apprenticeship { Id = 2, AgreementStatus = AgreementStatus.EmployerAgreed },
                                        new Domain.Entities.Apprenticeship { Id = 3, AgreementStatus = AgreementStatus.ProviderAgreed },
                                        new Domain.Entities.Apprenticeship { Id = 4, AgreementStatus = AgreementStatus.NotAgreed }
                                    }
                        };

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
            var expectedOriginalState = JsonConvert.SerializeObject(testCommitment);
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);
            _mockApprenticeshipRepository.Setup(x => x.CreateApprenticeship(It.IsAny<Domain.Entities.Apprenticeship>())).ReturnsAsync(expectedApprenticeshipId);

            await _handler.Handle(_exampleValidRequest);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().ChangeType == CommitmentChangeType.CreatedApprenticeship.ToString() &&
                                y.First().CommitmentId == testCommitment.Id &&
                                y.First().ApprenticeshipId == null &&
                                y.First().OriginalState == expectedOriginalState &&
                                y.First().UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == expectedOriginalState &&
                                y.First().UserId == _exampleValidRequest.UserId &&
                                y.First().ProviderId == _providerId &&
                                y.First().EmployerAccountId == _employerAccountId &&
                                y.First().UpdatedByName == _exampleValidRequest.UserName)), Times.Once);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.Last().ChangeType == ApprenticeshipChangeType.Created.ToString() &&
                                y.Last().CommitmentId == null &&
                                y.Last().ApprenticeshipId == expectedApprenticeshipId &&
                                y.Last().OriginalState == null &&
                                y.Last().UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString() &&
                                y.Last().UpdatedState != null &&
                                y.Last().UserId == _exampleValidRequest.UserId &&
                                y.Last().ProviderId ==_providerId &&
                                y.Last().EmployerAccountId == _employerAccountId &&
                                y.Last().UpdatedByName == _exampleValidRequest.UserName)), Times.Once);
        }

        private void AssertMappingIsCorrect(Domain.Entities.Apprenticeship argument)
        {
            argument.Id.Should().Be(_exampleValidRequest.Apprenticeship.Id);
            argument.FirstName.Should().Be(_exampleValidRequest.Apprenticeship.FirstName);
            argument.LastName.Should().Be(_exampleValidRequest.Apprenticeship.LastName);
            argument.CommitmentId.Should().Be(_exampleValidRequest.CommitmentId);
            argument.Cost.Should().Be(_exampleValidRequest.Apprenticeship.Cost);
            argument.StartDate.Should().Be(_exampleValidRequest.Apprenticeship.StartDate);
            argument.EndDate.Should().Be(_exampleValidRequest.Apprenticeship.EndDate);
            argument.TrainingType.Should().Be((TrainingType) _exampleValidRequest.Apprenticeship.TrainingType);
            argument.TrainingCode.Should().Be(_exampleValidRequest.Apprenticeship.TrainingCode);
            argument.TrainingName.Should().Be(_exampleValidRequest.Apprenticeship.TrainingName);
            argument.ULN.Should().Be(_exampleValidRequest.Apprenticeship.ULN);
            argument.PaymentStatus.Should().Be(PaymentStatus.PendingApproval);
            argument.AgreementStatus.Should().Be((AgreementStatus) _exampleValidRequest.Apprenticeship.AgreementStatus);
        }
    }
}
