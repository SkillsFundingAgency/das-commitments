using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class ChangedAccountNameEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingChangedAccountNameEvent_ThenShouldSendUpdateAccountNameCommand()
        {
            var fixture = new ChangedAccountNameEventHandlerTestsFixture();
            await fixture.Handle();
            
            fixture.VerifySend<UpdateAccountNameCommand>((c, m) =>
                c.AccountId == m.AccountId && c.Name == m.CurrentName && c.Created == m.Created);
        }
    }

    public class ChangedAccountNameEventHandlerTestsFixture : EventHandlerTestsFixture<ChangedAccountNameEvent, ChangedAccountNameEventHandler>
    {
    }
}