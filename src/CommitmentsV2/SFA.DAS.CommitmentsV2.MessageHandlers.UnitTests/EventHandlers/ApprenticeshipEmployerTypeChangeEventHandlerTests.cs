using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLevyStatus;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using CommonEmployerType = SFA.DAS.Common.Domain.Types.ApprenticeshipEmployerType;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ApprenticeshipEmployerTypeChangeEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenEmployerTypeIsLevy_ThenSendsUpdateAccountLevyStatusCommandWithLevy()
        {
            var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture();
            fixture.SetEmployerType(CommonEmployerType.Levy);

            await fixture.Handle();

            fixture.VerifyUpdateAccountLevyStatusCommandSent(ApprenticeshipEmployerType.Levy);
        }

        [Test]
        public async Task Handle_WhenEmployerTypeIsNonLevy_ThenSendsUpdateAccountLevyStatusCommandWithNonLevy()
        {
            var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture();
            fixture.SetEmployerType(CommonEmployerType.NonLevy);

            await fixture.Handle();

            fixture.VerifyUpdateAccountLevyStatusCommandSent(ApprenticeshipEmployerType.NonLevy);
        }

        [Test]
        public async Task Handle_WhenEmployerTypeIsUnknown_ThenDoesNotSendCommand()
        {
            var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture();
            fixture.SetEmployerType(CommonEmployerType.Unknown);

            await fixture.Handle();

            fixture.VerifyNoCommandSent();
        }
    }

    public class ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture
    {
        public Mock<IMediator> Mediator { get; set; }
        public ApprenticeshipEmployerTypeChangeEventHandler Sut { get; set; }
        public ApprenticeshipEmployerTypeChangeEvent Message { get; set; }

        public ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture()
        {
            Mediator = new Mock<IMediator>();
            Message = new ApprenticeshipEmployerTypeChangeEvent
            {
                AccountId = 1001,
                Created = DateTime.UtcNow
            };
            Sut = new ApprenticeshipEmployerTypeChangeEventHandler(Mediator.Object, Mock.Of<ILogger<ApprenticeshipEmployerTypeChangeEventHandler>>());
        }

        public ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture SetEmployerType(CommonEmployerType employerType)
        {
            Message.ApprenticeshipEmployerType = employerType;
            return this;
        }

        public Task Handle()
        {
            return Sut.Handle(Message, Mock.Of<IMessageHandlerContext>());
        }

        public void VerifyUpdateAccountLevyStatusCommandSent(ApprenticeshipEmployerType expectedLevyStatus)
        {
            Mediator.Verify(x =>
                x.Send(It.Is<UpdateAccountLevyStatusCommand>(p =>
                        p.AccountId == Message.AccountId &&
                        p.LevyStatus == expectedLevyStatus),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        public void VerifyNoCommandSent()
        {
            Mediator.Verify(x =>
                x.Send(It.IsAny<UpdateAccountLevyStatusCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
