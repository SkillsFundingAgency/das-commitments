using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class UpdatedLegalEntityEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingUpdatedLegalEntityEvent_ThenShouldSendUpdateAccountLegalEntityNameCommand()
        {
            var fixture = new UpdatedLegalEntityEventHandlerTestsFixture();
            await fixture.Handle();
            
            fixture.VerifySend<UpdateAccountLegalEntityNameCommand>((c, m) =>
                c.AccountLegalEntityId == m.AccountLegalEntityId && c.Name == m.Name && c.Created == m.Created);
        }
    }

    public class UpdatedLegalEntityEventHandlerTestsFixture : EventHandlerTestsFixture<UpdatedLegalEntityEvent, UpdatedLegalEntityEventHandler>
    {
    }
}