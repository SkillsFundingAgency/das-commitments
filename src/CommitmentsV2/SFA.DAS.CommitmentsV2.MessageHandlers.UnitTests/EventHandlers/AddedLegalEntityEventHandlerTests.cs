using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class AddedLegalEntityEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingAddedLegalEntityEvent_ThenShouldSendAddAccountLegalEntityCommand()
        {
            var fixture = new AddedLegalEntityEventHandlerTestsFixture();
            await fixture.Handle();
            
            fixture.VerifySend<AddAccountLegalEntityCommand>((c, m) =>
                c.AccountId == m.AccountId &&
                c.AccountLegalEntityId == m.AccountLegalEntityId &&
                c.MaLegalEntityId == m.LegalEntityId &&
                c.AccountLegalEntityPublicHashedId == m.AccountLegalEntityPublicHashedId &&
                c.OrganisationName == m.OrganisationName &&
                c.Created == m.Created);
        }
    }

    public class AddedLegalEntityEventHandlerTestsFixture : EventHandlerTestsFixture<AddedLegalEntityEvent, AddedLegalEntityEventHandler>
    {
    }
}