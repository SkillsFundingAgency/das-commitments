using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Infrastructure.Data;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Data.ParentChildrenMapperTests
{
    [TestFixture]
    public class WhenMapping
    {
        [Test]
        public void ThenANullLookupParameterThrowsAnArgumentNullException()
        {
            var mapper = new ParentChildrenMapper<object, object>();

            Action act = () => mapper.Map(null, x => x, x => new List<object>());

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ThenIfLookupIsEmptyShouldAddParentObjectAndWithChild()
        {
            var mapper = new ParentChildrenMapper<Parent, Child>();
            var lookup = new Dictionary<int, Parent>();
            var parent = new Parent { Id = 2, Children = new List<Child>() };
            var child = new Child();

            var result = mapper.Map(lookup, x => x.Id, x => x.Children)(parent, child);

            result.Should().Be(parent);
            result.Children.Should().Contain(child);
            lookup.Values.Should().Contain(parent);
        }

        [Test]
        public void ThenIfLookupAlreadyContainsEntryWithIdOfParentShouldAddNewChildToExistingParent()
        {
            var mapper = new ParentChildrenMapper<Parent, Child>();
            var lookup = new Dictionary<int, Parent>();
            var existingEntry = new Parent { Id = 2, Children = new List<Child> { new Child() } };
            lookup.Add(existingEntry.Id, existingEntry);

            var parent = new Parent { Id = 2, Children = new List<Child>() };
            var child = new Child();

            var result = mapper.Map(lookup, x => x.Id, x => x.Children)(parent, child);

            lookup.Values.Count.Should().Be(1);
            lookup.Values.Single().Children.Count.Should().Be(2);
        }

        private class Parent
        {
            public int Id { get; set; }

            public IList<Child> Children { get; set; } 
        }

        private class Child
        {
        }
    }
}
