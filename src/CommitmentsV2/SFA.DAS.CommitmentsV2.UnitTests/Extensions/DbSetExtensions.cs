using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions
{
    public static class DbSetExtensions
    {
        public static DbSet<T> MockFromSql<T>(this DbSet<T> dbSet, SpAsyncEnumerableQueryable<T> spItems) where T : class
        {
            var queryProviderMock = new Mock<IQueryProvider>();
            queryProviderMock.Setup(p => p.CreateQuery<T>(It.IsAny<MethodCallExpression>()))
                .Returns<MethodCallExpression>(x =>
                {
                    return spItems;
                });

            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>()
                .SetupGet(q => q.Provider)
                .Returns(() =>
                {
                    return queryProviderMock.Object;
                });

            dbSetMock.As<IQueryable<T>>()
                .Setup(q => q.Expression)
                .Returns(Expression.Constant(dbSetMock.Object));
            return dbSetMock.Object;
        }
    }

    public class SpAsyncEnumerableQueryable<T> : IAsyncEnumerable<T>, IQueryable<T>
    {
        private IAsyncEnumerable<T> _spItems;
        public Expression Expression => throw new NotImplementedException();
        public Type ElementType => throw new NotImplementedException();
        public IQueryProvider Provider => throw new NotImplementedException();

        public SpAsyncEnumerableQueryable(params T[] spItems)
        {
            _spItems = AsyncEnumerable.ToAsyncEnumerable(spItems);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _spItems.ToEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            return _spItems.GetEnumerator();
        }
    }
}
