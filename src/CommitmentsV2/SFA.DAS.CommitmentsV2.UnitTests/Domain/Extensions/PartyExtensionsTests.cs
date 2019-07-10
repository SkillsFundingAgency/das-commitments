﻿using System;
using System.ComponentModel;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.Extensions
{
    public class PartyExtensionsTests
    {
        [TestCase(Party.Provider, EditStatus.ProviderOnly)]
        [TestCase(Party.Employer, EditStatus.EmployerOnly)]
        public void TestValidEditStatusMapping(Party party, EditStatus expectedResult)
        {
            var result = party.ToEditStatus();
            Assert.IsTrue(result == expectedResult);
        }

        [TestCase(Party.TransferSender)]
        [TestCase(Party.None)]
        public void TestInvalidEditStatusMapping(Party party)
        {
            Assert.Throws<ArgumentException>(() => party.ToEditStatus());
        }

        [TestCase(Party.Provider, Originator.Provider)]
        [TestCase(Party.Employer, Originator.Employer)]
        public void TestValidOriginatorMapping(Party party, Originator expectedResult)
        {
            var result = party.ToOriginator();
            Assert.IsTrue(result == expectedResult);
        }

        public void TestInvalidOriginatorMapping(Party party)
        {
            Assert.Throws<ArgumentException>(() => party.ToOriginator());
        }
    }
}