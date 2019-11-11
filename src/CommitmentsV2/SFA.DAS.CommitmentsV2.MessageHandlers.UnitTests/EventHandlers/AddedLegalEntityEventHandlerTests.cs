using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class AddedLegalEntityEventHandlerTests : FluentTest<AddedLegalEntityEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingAddedLegalEntityEvent_ThenShouldSendAddAccountLegalEntityCommand()
        {
            return TestAsync(f => f.Handle(), f => f.VerifySend<AddAccountLegalEntityCommand>((c, m) =>
                c.AccountId == m.AccountId &&
                c.AccountLegalEntityId == m.AccountLegalEntityId &&
                c.MaLegalEntityId == m.LegalEntityId &&
                c.AccountLegalEntityPublicHashedId == m.AccountLegalEntityPublicHashedId &&
                c.OrganisationName == m.OrganisationName &&
                c.Created == m.Created));
        }
    }

    public class AddedLegalEntityEventHandlerTestsFixture : EventHandlerTestsFixture<AddedLegalEntityEvent, AddedLegalEntityEventHandler>
    {
    }
}