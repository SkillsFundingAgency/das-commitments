using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.UnitOfWork.Pipeline;

namespace SFA.DAS.CommitmentsV2.Data
{
    public class HistoryUnitOfWork : IUnitOfWork
    {
        private readonly Lazy<CommitmentsDbContext> _db;
        private readonly ICurrentDateTime _currentDateTime;

        public HistoryUnitOfWork(Lazy<CommitmentsDbContext> db, ICurrentDateTime currentDateTime)
        {
            _db = db;
            _currentDateTime = currentDateTime;
        }

        public Task CommitAsync(Func<Task> next)
        {
            var historyItems = _db.Value.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .Select(e => new HistoryItemV2
                {
                    TransactionId = _db.Value.Database.CurrentTransaction.TransactionId,
                    EntityType = e.Entity.GetType().FullName,
                    EntityState = e.State.ToString(),
                    Original = e.State == EntityState.Added ? null : e.Properties
                        .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)
                        .ToJson(),
                    Modified = e.State == EntityState.Deleted ? null : e.Properties
                        .Where(p => e.State == EntityState.Added || p.IsModified)
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
                        .ToJson(),
                    CreatedOn = _currentDateTime.UtcNow
                })
                .ToList();
            
            _db.Value.HistoryItemsV2.AddRange(historyItems);

            return next();
        }
    }
}