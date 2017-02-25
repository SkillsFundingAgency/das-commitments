using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;
using TrainingType = SFA.DAS.Commitments.Domain.Entities.TrainingType;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeship
{
    [TestFixture]
    public sealed class WhenCreatingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private CreateApprenticeshipCommandHandler _handler;
        private CreateApprenticeshipCommand _exampleValidRequest;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;

        private Mock<IHistoryRepository> _mockHistoryRepository;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _handler = new CreateApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object, 
                new CreateApprenticeshipValidator(), 
                _mockApprenticeshipEvents.Object, 
                Mock.Of<ICommitmentsLogger>(), 
                _mockHistoryRepository.Object);

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
                .Create();

            _exampleValidRequest = new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                Apprenticeship = populatedApprenticeship
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

            _mockCommitmentRespository.Verify(x => x.CreateApprenticeship(It.IsAny<Domain.Entities.Apprenticeship>()));
        }

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.Provider)]
        public async Task ThenShouldCallTheHistoryRepository(CallerType callerType)
        {
            _exampleValidRequest.Caller.CallerType = callerType;
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id,
                EmployerAccountId = _exampleValidRequest.Caller.Id
            });

            await _handler.Handle(_exampleValidRequest);

            _mockHistoryRepository.Verify(x => x.CreateApprenticeship(
                It.Is<ApprenticeshipHistoryItem>(arg 
                    => arg.ChangeType == ApprenticeshipChangeType.Created
                    && arg.UpdatedByRole == callerType
                    && arg.UserId == _exampleValidRequest.Caller.Id)
                ), Times.Once);

            _mockHistoryRepository.Verify(x => x.CreateCommitmentHistory(
                It.Is<CommitmentHistoryItem>(arg 
                    => arg.ChangeType == CommitmentChangeType.CreateApprenticeship
                    && arg.UpdatedByRole == callerType
                    && arg.UserId == _exampleValidRequest.Caller.Id)
                ), Times.Once);
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

            _mockCommitmentRespository.Verify(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Domain.Entities.AgreementStatus>()), Times.Exactly(2));
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
            _mockCommitmentRespository.Setup(x => x.CreateApprenticeship(It.IsAny<Domain.Entities.Apprenticeship>()))
                .ReturnsAsync(_exampleValidRequest.Apprenticeship.Id)
                .Callback<Domain.Entities.Apprenticeship>(x => argument = x);

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
            _mockCommitmentRespository.Setup(x => x.CreateApprenticeship(It.IsAny<Domain.Entities.Apprenticeship>())).ReturnsAsync(expectedApprenticeshipId);

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
