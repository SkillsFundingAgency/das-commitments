using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.ProviderCommitments.Extensions;
using SFA.DAS.Testing;

namespace SFA.DAS.ProviderCommitments.UnitTests.Extensions
{
    [TestFixture]
    [Parallelizable]
    public class ExceptionExtensionsTests : FluentTest<object>
    {
        [Test]
        public void GetAggregateMessage_WhenExceptionHasNoInnerExceptions_ThenShouldReturnExceptionMessage()
        {
            Test(f => new Exception($"One").AggregateMessages(), (f, r) => r.Should().Be($"One"));
        }

        [Test]
        public void GetAggregateMessage_WhenExceptionHasInnerExceptions_ThenShouldReturnExceptionAndInnerExceptionMessages()
        {
            Test(f => new Exception($"One", new Exception("Two", new Exception("Three"))).AggregateMessages(), (f, r) => r.Should().Be($"One{Environment.NewLine}Two{Environment.NewLine}Three"));
        }
    }
}