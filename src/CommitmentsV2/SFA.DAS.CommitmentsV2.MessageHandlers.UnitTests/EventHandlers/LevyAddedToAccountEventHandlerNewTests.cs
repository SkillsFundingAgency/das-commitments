using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLevyStatus;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Types;
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
            fixture.VerifyUpdateAccountLevyStatusCommandSent();
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

        public void VerifyUpdateAccountLevyStatusCommandSent()
        {
            Mediator.Verify(x =>
                x.Send(It.Is<UpdateAccountLevyStatusCommand>(p =>
                        p.AccountId == ALevyAddedToAccount.AccountId &&
                        p.LevyStatus == ApprenticeshipEmployerType.Levy),
                    It.IsAny<CancellationToken>()));
        }
    }
}
