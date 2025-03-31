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
        public LevyAddedToAccountEventHandler Sut;
        public LevyAddedToAccountEvent ALevyAddedToAccount;

        public LevyAddedToAccountEventHandlerNewTestsFixture()
        {
            var autoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            ALevyAddedToAccount = autoFixture.Create<LevyAddedToAccountEvent>();

            Sut = new LevyAddedToAccountEventHandler(Mediator.Object, Mock.Of<ILogger<LevyAddedToAccountEventHandler>>());
        }

        public Task Handle()
        {
            return Sut.Handle(ALevyAddedToAccount, Mock.Of<IMessageHandlerContext>());
        }

        public void VerifyUpdateLevyStatusToLevyCommandSent()
        {
            Mediator.Verify(x =>
                x.Send(It.Is<UpdateLevyStatusToLevyCommand>(p => p.AccountId == ALevyAddedToAccount.AccountId),
                    It.IsAny<CancellationToken>()));
        }
    }
}
