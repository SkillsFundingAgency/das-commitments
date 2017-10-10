using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    public sealed class WhenStoppingAnInvalidApprenticeship
    {
        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockEventsApi = new Mock<IApprenticeshipEvents>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockDataLockRepository = new Mock<IDataLockRepository>();
            _mockCurrentDateTime = new Mock<ICurrentDateTime>();
            _mockAcademicYearValidator = new Mock<IAcademicYearValidator>();
            _mockCommitmentsLogger = new Mock<ICommitmentsLogger>();

            _handler = new StopApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                new ApprenticeshipStatusChangeCommandValidator(),
                _mockCurrentDateTime.Object,
                _mockEventsApi.Object,
                _mockCommitmentsLogger.Object,
                _mockHistoryRepository.Object,
                _mockDataLockRepository.Object,
                _mockAcademicYearValidator.Object);

            _exampleValidRequest = new StopApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = DateTime.UtcNow.Date.AddMonths(6),
                UserName = "Bob"
            };

            _testApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.Date.AddMonths(6)
            };

            _mockCurrentDateTime.SetupGet(x => x.Now)
                .Returns(DateTime.UtcNow);

            _mockApprenticeshipRespository.Setup(x =>
                    x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(_testApprenticeship);

            _mockApprenticeshipRespository.Setup(x =>
                    x.UpdateApprenticeshipStatus(
                        It.Is<long>(c => c == _testApprenticeship.CommitmentId),
                        It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                        It.Is<PaymentStatus>(s => s == PaymentStatus.Withdrawn)))
                .Returns(Task.FromResult(new object()));

            _mockDataLockRepository.Setup(x => x.GetDataLocks(_exampleValidRequest.ApprenticeshipId))
                .ReturnsAsync(new List<DataLockStatus>());

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(
                    It.Is<long>(c => c == _testApprenticeship.CommitmentId)))
                .ReturnsAsync(new Commitment
                {
                    Id = 123L,
                    EmployerAccountId = 0L
                });
        }

        private StopApprenticeshipCommand _exampleValidRequest;
        private Apprenticeship _testApprenticeship;
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private Mock<ICurrentDateTime> _mockCurrentDateTime;
        private Mock<IApprenticeshipEvents> _mockEventsApi;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IDataLockRepository> _mockDataLockRepository;
        private StopApprenticeshipCommandHandler _handler;
        private Mock<IAcademicYearValidator> _mockAcademicYearValidator;
        private Mock<ICommitmentsLogger> _mockCommitmentsLogger;




        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            _testApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<Exception>();
        }



        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }


        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }
       
    }
}