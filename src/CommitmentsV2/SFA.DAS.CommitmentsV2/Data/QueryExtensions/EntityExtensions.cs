﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.CommitmentsV2.Data.QueryExtensions
{
    public static class EntityExtensions
    {
        public static Task<TResponse> GetById<TEntity, TResponse>(
            this DbSet<TEntity> query,
            Expression<Func<TEntity, bool>> where,
            Expression<Func<TEntity, TResponse>> select,
            CancellationToken cancellationToken) where TEntity : class
        {
            return query
                .AsNoTracking()
                .Where(where)
                .Select(select)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}