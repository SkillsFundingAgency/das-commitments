using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Commitments.EFCoreTester.Config;
using SFA.DAS.Commitments.EFCoreTester.Data;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;

namespace SFA.DAS.Commitments.EFCoreTester.Commands
{
    public class ReadEFCommand : ICommand
    {
        private readonly ITimer _timer;
        private readonly ReadConfig _config;

        public ReadEFCommand(IConfigProvider configProvider, ITimer timer)
        {
            _config = configProvider.Get<ReadConfig>();
            _timer = timer;
        }

        public Task DoAsync(CancellationToken cancellationToken)
        {
            using (var db = CreateDbContext())
            {
                if (_config.NoTracking)
                {
                    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                }

                _timer.Time("Read drafts", () => db.DraftApprenticeships.ToList());
                _timer.Time("Read confirmed", () => db.ConfirmedApprenticeships.ToList());
            }

            return Task.CompletedTask;
        }

        private ProviderDbContext CreateDbContext()
        {
            return _timer.Time("Create DB Context", () => new ProviderDbContext());
        }
    }
}
