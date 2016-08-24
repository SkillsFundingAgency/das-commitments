using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public sealed class ParentChildrenMapper<Parent, Child>
    {
        public Func<Parent, Child, Parent> Map<T>(Dictionary<T, Parent> lookup, Func<Parent, T> parentIdentifierProperty, Func<Parent, IList<Child>> parentChildrenProperty)
        {
            if (lookup == null)
            {
                throw new ArgumentNullException(nameof(lookup));
            }

            return (x, y) =>
            {
                Parent parent;
                if (!lookup.TryGetValue(parentIdentifierProperty.Invoke(x), out parent))
                {
                    lookup.Add(parentIdentifierProperty.Invoke(x), parent = x);
                }

                var children = parentChildrenProperty.Invoke(parent);
                if (children == null)
                {
                    children = new List<Child>();
                }
                children.Add(y);

                return parent;
            };
        }
    }
}
