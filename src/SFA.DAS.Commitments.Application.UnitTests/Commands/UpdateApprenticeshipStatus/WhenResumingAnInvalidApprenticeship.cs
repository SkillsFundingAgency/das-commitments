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
    public sealed class WhenResumingAnInvalidApprenticeship : WhenResumingAnApprenticeship
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

          
            MockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2017, 6, 1));


            ExampleValidRequest = new ResumeApprenticeshipCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                DateOfChange = MockCurrentDateTime.Object.Now.Date,
                UserName = "Bob"
            };

            TestApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = PaymentStatus.Paused,
                PauseDate = MockCurrentDateTime.Object.Now.AddMonths(-2).Date,
                StartDate = MockCurrentDateTime.Object.Now.Date.AddMonths(6)
            };


            MockApprenticeshipRespository.Setup(x => x.GetApprenticeship(
                    It.Is<long>(y => y == ExampleValidRequest.ApprenticeshipId)
                ))
                .ReturnsAsync(TestApprenticeship);

            MockApprenticeshipRespository.Setup(x => x.UpdateApprenticeshipStatus(
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
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsExceptionAndNoV2ApprenticeshipResumedEventPublished(PaymentStatus initial)
        {
            TestApprenticeship.PaymentStatus = initial;

            Func<Task> act = async () => await Handler.Handle(ExampleValidRequest);

            act.ShouldThrow<Exception>();

            MockV2EventsPublisher.Verify(x => x.PublishApprenticeshipResumed(
                It.IsAny<Commitment>(),
                It.IsAny<Apprenticeship>()), Times.Never);
        }
    }
}