using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class CreatedAccountEventHandlerTests : FluentTest<CreatedAccountEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingCreatedAccountEvent_ThenShouldSendCreateAccountLegalEntityCommand()
        {
            return TestAsync(f => f.Handle(), f => f.VerifySend<CreateAccountCommand>((c, m) =>
                c.AccountId == m.AccountId &&
                c.HashedId == m.HashedId &&
                c.PublicHashedId == m.PublicHashedId &&
                c.Name == m.Name &&
                c.Created == m.Created));
        }
    }

    public class
        CreatedAccountEventHandlerTestsFixture : EventHandlerTestsFixture<CreatedAccountEvent,
            CreatedAccountEventHandler>
    {
    }
}