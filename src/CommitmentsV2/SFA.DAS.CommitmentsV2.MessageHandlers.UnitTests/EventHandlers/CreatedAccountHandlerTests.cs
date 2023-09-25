using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class CreatedAccountEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingCreatedAccountEvent_ThenShouldSendCreateAccountLegalEntityCommand()
        {
            var fixture = new CreatedAccountEventHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifySend<CreateAccountCommand>((command, createdAccountEvent) =>
                command.AccountId == createdAccountEvent.AccountId &&
                command.HashedId == createdAccountEvent.HashedId &&
                command.PublicHashedId == createdAccountEvent.PublicHashedId &&
                command.Name == createdAccountEvent.Name &&
                command.Created == createdAccountEvent.Created);
        }
    }

    public class CreatedAccountEventHandlerTestsFixture : EventHandlerTestsFixture<CreatedAccountEvent, CreatedAccountEventHandler>
    {
    }
}
