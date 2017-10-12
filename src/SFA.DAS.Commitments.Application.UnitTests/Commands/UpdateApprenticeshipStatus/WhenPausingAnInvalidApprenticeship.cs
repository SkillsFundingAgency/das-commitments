using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenPausingAnInvalidApprenticeship : WhenPausingAnApprenticeshipBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();


            ExampleValidRequest = new PauseApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = DateTime.Now.Date,
                UserName = "Bob"
            };

            TestApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.Date.AddMonths(6)
            };

            MockCurrentDateTime.SetupGet(x => x.Now).Returns(DateTime.UtcNow);

            MockApprenticeshipRespository
                .Setup(x => x.GetApprenticeship(It.Is<long>(y => y == ExampleValidRequest.ApprenticeshipId)))
                .ReturnsAsync(TestApprenticeship);

            MockApprenticeshipRespository
                .Setup(x => x.UpdateApprenticeshipStatus(TestApprenticeship.CommitmentId,
                    ExampleValidRequest.ApprenticeshipId,
                    PaymentStatus.Paused))
                .Returns(Task.FromResult(new object()));
        }


        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial)
        {
            TestApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<Exception>();
        }
    }
}