using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Language.Flow;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.Testing.EntityFrameworkCore;

namespace SFA.DAS.CommitmentsV2.UnitTests
{
    public static class MoqExtensions
    {
        public static IReturnsResult<IProviderCommitmentsDbContext> ReturnsDbSet<TEntity>(this ISetup<IProviderCommitmentsDbContext, DbSet<TEntity>> setupResult, IEnumerable<TEntity> entities) where TEntity : class
        {
            var entitiesAsQueryable = entities.AsQueryable();
            var dbSetMock = new Mock<DbSet<TEntity>>();

            dbSetMock.As<IAsyncEnumerable<TEntity>>()
                .Setup(m => m.GetEnumerator())
                .Returns(new InMemoryDbAsyncEnumerator<TEntity>(entitiesAsQueryable.GetEnumerator()));

            dbSetMock.As<IQueryable<TEntity>>()
                .Setup(m => m.Provider)
                .Returns(new InMemoryAsyncQueryProvider<TEntity>(entitiesAsQueryable.Provider));

            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(entitiesAsQueryable.Expression);
            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(entitiesAsQueryable.ElementType);
            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(entitiesAsQueryable.GetEnumerator());

            return setupResult.Returns(dbSetMock.Object);
        }

        public static IReturnsResult<ICommitmentsReadOnlyDbContext> ReturnsDbSet<TEntity>(this ISetup<ICommitmentsReadOnlyDbContext, DbSet<TEntity>> setupResult, IEnumerable<TEntity> entities) where TEntity : class
        {
            var entitiesAsQueryable = entities.AsQueryable();
            var dbSetMock = new Mock<DbSet<TEntity>>();

            dbSetMock.As<IAsyncEnumerable<TEntity>>()
                .Setup(m => m.GetEnumerator())
                .Returns(new InMemoryDbAsyncEnumerator<TEntity>(entitiesAsQueryable.GetEnumerator()));

            dbSetMock.As<IQueryable<TEntity>>()
                .Setup(m => m.Provider)
                .Returns(new InMemoryAsyncQueryProvider<TEntity>(entitiesAsQueryable.Provider));

            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(entitiesAsQueryable.Expression);
            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(entitiesAsQueryable.ElementType);
            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(entitiesAsQueryable.GetEnumerator());

            return setupResult.Returns(dbSetMock.Object);
        }
    }
}