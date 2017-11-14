using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    public sealed class WhenStoppingAnInvalidApprenticeship : WhenStoppingAnApprenticeship
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            ExampleValidRequest = new StopApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = DateTime.UtcNow.Date.AddMonths(6),
                UserName = "Bob"
            };

            TestApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.Date.AddMonths(6)
            };

            MockCurrentDateTime.SetupGet(x => x.Now)
                .Returns(DateTime.UtcNow);

            MockApprenticeshipRespository.Setup(x =>
                    x.GetApprenticeship(It.Is<long>(y => y == ExampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(TestApprenticeship);

            MockApprenticeshipRespository.Setup(x =>
                    x.UpdateApprenticeshipStatus(
                        It.Is<long>(c => c == TestApprenticeship.CommitmentId),
                        It.Is<long>(a => a == ExampleValidRequest.ApprenticeshipId),
                        It.Is<PaymentStatus>(s => s == PaymentStatus.Withdrawn)))
                .Returns(Task.FromResult(new object()));

            MockDataLockRepository.Setup(x => x.GetDataLocks(ExampleValidRequest.ApprenticeshipId, false))
                .ReturnsAsync(new List<DataLockStatus>());

            MockCommitmentRespository.Setup(x => x.GetCommitmentById(
                    It.Is<long>(c => c == TestApprenticeship.CommitmentId)))
                .ReturnsAsync(new Commitment
                {
                    Id = 123L,
                    EmployerAccountId = 0L
                });
        }


        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            TestApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<Exception>();
        }


        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            ExampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }


        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }
    }
}