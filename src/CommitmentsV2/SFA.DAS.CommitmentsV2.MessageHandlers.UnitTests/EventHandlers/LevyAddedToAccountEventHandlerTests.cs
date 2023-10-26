using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class LevyAddedToAccountEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenLevyAddedToAccountIsRaised_LevyStatusIsSetToLevy()
        {
            var fixture = new LevyAddedToAccountEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.VerifyUpdateLevyStatusToLevyCommandSent();
        }
    }

    public class LevyAddedToAccountEventHandlerTestsFixture
    {
        public Mock<IMediator> Mediator { get; set; }
        public LevyAddedToAccountEventHandler Sut;
        public LevyAddedToAccount LevyAddedToAccount;

        public LevyAddedToAccountEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            Mediator = new Mock<IMediator>();
            LevyAddedToAccount = autoFixture.Create<LevyAddedToAccount>();

            Sut = new LevyAddedToAccountEventHandler(Mediator.Object, Mock.Of<ILogger<LevyAddedToAccountEventHandler>>());
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
