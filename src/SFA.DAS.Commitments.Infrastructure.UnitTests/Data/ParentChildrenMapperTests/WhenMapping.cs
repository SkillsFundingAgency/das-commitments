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
        private ParentChildrenMapper<Parent, Child> _mapper;

        [SetUp]
        public void SetUp()
        {
            _mapper = new ParentChildrenMapper<Parent, Child>();
        }

        [Test]
        public void ThenANullLookupParameterThrowsAnArgumentNullException()
        {
            Action act = () => _mapper.Map(null, x => x, x => new List<Child>());

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ThenIfLookupIsEmptyShouldAddParentObjectAndWithChild()
        {
            var lookup = new Dictionary<int, Parent>();
            var parent = new Parent { Id = 2, Children = new List<Child>() };
            var child = new Child();

            var result = _mapper.Map(lookup, x => x.Id, x => x.Children)(parent, child);

            result.Should().Be(parent);
            result.Children.Should().Contain(child);
            lookup.Values.Should().Contain(parent);
        }

        [Test]
        public void ThenIfLookupAlreadyContainsEntryWithIdOfParentShouldAddNewChildToExistingParent()
        {
            var lookup = new Dictionary<int, Parent>();
            var existingEntry = new Parent { Id = 2, Children = new List<Child> { new Child() } };
            lookup.Add(existingEntry.Id, existingEntry);

            var parent = new Parent { Id = 2, Children = new List<Child>() };
            var child = new Child();

            var result = _mapper.Map(lookup, x => x.Id, x => x.Children)(parent, child);

            lookup.Values.Count.Should().Be(1);
            lookup.Values.Single().Children.Count.Should().Be(2);
        }

        [Test]
        public void ThenIfChildIsNullDontAddItToTheParentAsAChild()
        {
            var lookup = new Dictionary<int, Parent>();

            var parent = new Parent { Id = 2, Children = new List<Child>() };
            Child child = null;

            var result = _mapper.Map(lookup, x => x.Id, x => x.Children)(parent, child);

            lookup.Values.Single().Children.Count.Should().Be(0);
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
