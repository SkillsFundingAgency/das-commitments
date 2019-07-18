using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class ChangedAccountNameEventHandlerTests : FluentTest<ChangedAccountNameEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingChangedAccountNameEvent_ThenShouldSendUpdateAccountNameCommand()
        {
            return TestAsync(f => f.Handle(), f => f.VerifySend<UpdateAccountNameCommand>((c, m) =>
                c.AccountId == m.AccountId && c.Name == m.CurrentName && c.Created == m.Created));
        }
    }

    public class
        ChangedAccountNameEventHandlerTestsFixture : EventHandlerTestsFixture<ChangedAccountNameEvent,
            ChangedAccountNameEventHandler>
    {
    }
}