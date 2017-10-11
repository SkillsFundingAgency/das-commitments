using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenResumingAnInvalidApprenticeship
    {
        private Mock<IAcademicYearDateProvider> _mockAcademicYearDateProvider;
        private Mock<IAcademicYearValidator> _mockAcademicYearValidator;
        private ResumeApprenticeshipCommand _exampleValidRequest;
        private Apprenticeship _testApprenticeship;
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private Mock<ICurrentDateTime> _mockCurrentDateTime;
        private Mock<IApprenticeshipEvents> _mockEventsApi;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private ResumeApprenticeshipCommandHandler _handler;
        private Mock<ICommitmentsLogger> _mockCommitmentsLogger;


        [SetUp]
        public void SetUp()
        {
            _mockAcademicYearDateProvider = new Mock<IAcademicYearDateProvider>();
            _mockAcademicYearValidator = new Mock<IAcademicYearValidator>();

            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockEventsApi = new Mock<IApprenticeshipEvents>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockCurrentDateTime = new Mock<ICurrentDateTime>();
            _mockCommitmentsLogger = new Mock<ICommitmentsLogger>();

            _handler = new ResumeApprenticeshipCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                new ApprenticeshipStatusChangeCommandValidator(),
                _mockCurrentDateTime.Object,
                _mockEventsApi.Object,
                _mockCommitmentsLogger.Object,
                _mockHistoryRepository.Object,
                _mockAcademicYearDateProvider.Object,
                _mockAcademicYearValidator.Object);


            _mockAcademicYearDateProvider.Setup((x) => x.CurrentAcademicYearStartDate)
                .Returns(new DateTime(2015, 8, 1));
            _mockAcademicYearDateProvider.Setup((x) => x.CurrentAcademicYearEndDate)
                .Returns(new DateTime(2016, 7, 31));
            _mockAcademicYearDateProvider.Setup((x) => x.LastAcademicYearFundingPeriod)
                .Returns(new DateTime(2016, 10, 19, 18, 0, 0, 0));

            _mockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2016, 6, 1));

      
            _exampleValidRequest = new ResumeApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = _mockCurrentDateTime.Object.Now.Date,
                UserName = "Bob"
            };

            _testApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Paused,
                PauseDate = _mockCurrentDateTime.Object.Now.AddMonths(-2).Date,
                StartDate = _mockCurrentDateTime.Object.Now.Date.AddMonths(6)
            };

            
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(
                    It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId)
                ))
                .ReturnsAsync(_testApprenticeship);

            _mockApprenticeshipRespository.Setup(x => x.UpdateApprenticeshipStatus(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<PaymentStatus>()
                ))
                .Returns(Task.FromResult(new object()));

        }

        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.PendingApproval)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            _testApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<Exception>();
        }

       

    }
}