﻿using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class RemovedLegalEntityEventHandlerTests : FluentTest<RemovedLegalEntityEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingRemoveLegalEntityEvent_ThenShouldSendRemoveAccountLegalEntityCommand()
        {
            return TestAsync(f => f.Handle(), f => f.VerifySend<RemoveAccountLegalEntityCommand>((c, m) =>
                c.AccountId == m.AccountId && c.AccountLegalEntityId == m.AccountLegalEntityId && c.Removed == m.Created));
        }
    }

    public class RemovedLegalEntityEventHandlerTestsFixture : EventHandlerTestsFixture<RemovedLegalEntityEvent, RemovedLegalEntityEventHandler>
    {
    }
}