using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class UpdatedLegalEntityEventHandlerTests : FluentTest<UpdatedLegalEntityEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingUpdatedLegalEntityEvent_ThenShouldSendUpdateAccountLegalEntityNameCommand()
        {
            return TestAsync(f => f.Handle(), f => f.VerifySend<UpdateAccountLegalEntityNameCommand>((c, m) =>
                c.AccountLegalEntityId == m.AccountLegalEntityId && c.Name == m.Name && c.Created == m.Created));
        }
    }

    public class UpdatedLegalEntityEventHandlerTestsFixture : EventHandlerTestsFixture<UpdatedLegalEntityEvent, UpdatedLegalEntityEventHandler>
    {
    }
}