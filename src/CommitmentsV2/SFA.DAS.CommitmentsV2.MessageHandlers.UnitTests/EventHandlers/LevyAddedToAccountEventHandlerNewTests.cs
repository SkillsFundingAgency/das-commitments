using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class LevyAddedToAccountEventHandlerNewTests
    {
        [Test]
        public async Task Handle_WhenLevyAddedToAccountIsRaised_LevyStatusIsSetToLevy()
        {
            var fixture = new LevyAddedToAccountEventHandlerNewTestsFixture();
            await fixture.Handle();
            fixture.VerifyUpdateLevyStatusToLevyCommandSent();
        }
    }

    public class LevyAddedToAccountEventHandlerNewTestsFixture
    {
        public Mock<IMediator> Mediator { get; set; }
        public LevyAddedToAccountEventHandlerNew Sut;
        public LevyAddedToAccountEvent LevyAddedToAccount;

        public LevyAddedToAccountEventHandlerNewTestsFixture()
        {
            var autoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            LevyAddedToAccount = autoFixture.Create<LevyAddedToAccountEvent>();

            Sut = new LevyAddedToAccountEventHandlerNew(Mediator.Object, Mock.Of<ILogger<LevyAddedToAccountEventHandlerNew>>());
        }

        public Task Handle()
        {
            return Sut.Handle(LevyAddedToAccount, Mock.Of<IMessageHandlerContext>());
        }

        public void VerifyUpdateLevyStatusToLevyCommandSent()
        {
            Mediator.Verify(x =>
                x.Send(It.Is<UpdateLevyStatusToLevyCommand>(p => p.AccountId == LevyAddedToAccount.AccountId),
                    It.IsAny<CancellationToken>()));
        }
    }
}
