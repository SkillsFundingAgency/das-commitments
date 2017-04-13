using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenPausingApprenticeship
    {
        // TODO: LWA - Complete tests
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private Mock<ICurrentDateTime> _mockCurrentDateTime;
        private Mock<IApprenticeshipEvents> _mockEventsApi;
        private UpdateApprenticeshipStatusCommandHandler _handler;
        private UpdateApprenticeshipStatusCommand _exampleValidRequest;
        private Apprenticeship _testApprenticeship;

        [SetUp]
        public void SetUp()
        {
            _exampleValidRequest = new UpdateApprenticeshipStatusCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                PaymentStatus = Api.Types.Apprenticeship.Types.PaymentStatus.Paused,
                DateOfChange = DateTime.Now.Date
            };

            _testApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.Date.AddMonths(-1)
            };

            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockCurrentDateTime = new Mock<ICurrentDateTime>();
            _mockEventsApi = new Mock<IApprenticeshipEvents>();

            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(_testApprenticeship);
            _mockApprenticeshipRespository.Setup(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<PaymentStatus>())).Returns(Task.FromResult(new object()));
            _mockCurrentDateTime.SetupGet(x => x.Now).Returns(DateTime.UtcNow);

            _handler = new UpdateApprenticeshipStatusCommandHandler(
                _mockCommitmentRespository.Object, 
                _mockApprenticeshipRespository.Object, 
                new UpdateApprenticeshipStatusValidator(), 
                _mockCurrentDateTime.Object,
                _mockEventsApi.Object,
                Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(x => x.PauseOrResumeApprenticeship(
                It.Is<long>(a => a == 123L),
                It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                It.Is<PaymentStatus>(a => a == PaymentStatus.Paused),
                It.Is<DateTime>(a => a == _exampleValidRequest.DateOfChange),
                It.Is<CallerType>(a => a == CallerType.Employer),
                It.Is<string>(a => a == _exampleValidRequest.UserId)));
        }

        //[Test]
        //public async Task ThenShouldSendAnApprenticeshipEvent()
        //{
        //    _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
        //    {
        //        Id = 123L,
        //        EmployerAccountId = _exampleValidRequest.AccountId
        //    });

        //    await _handler.Handle(_exampleValidRequest);

        //    _mockEventsApi.Verify(x => x.PublishChangeApprenticeshipStatusEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<PaymentStatus>()));
        //}

        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            _testApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<Exception>();
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsWaitingToStartAndChangeDateNotEqualToCurrentDate()
        {
            var startDate = DateTime.UtcNow.AddMonths(2).Date;
            _testApprenticeship.StartDate = startDate;

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _exampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Should().Be("Invalid Date of Change. Date should be todays date.");
        }

        [Test]
        public void ThenThrowsExceptionIfApprenticeshipIsInProgressAndChangeDateIsInFuture()
        {
            var startDate = DateTime.UtcNow.AddMonths(-2).Date;
            _testApprenticeship.StartDate = startDate;

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            _exampleValidRequest.DateOfChange = DateTime.UtcNow.AddMonths(1).Date;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>().Which.Message.Should().Be("Invalid Date of Change. Date should be todays date.");
        }
    }
}
