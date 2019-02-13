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
    public class ReadAllTablesCommand : ICommand
    {
        private readonly ITimer _timer;
        private readonly ReadConfig _config;

        public ReadAllTablesCommand(IConfigProvider configProvider, ITimer timer)
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

                _timer.Time("Read DraftApprenticeships", () => db.DraftApprenticeships.ToList());
                _timer.Time("Read ConfirmedApprenticeships", () => db.ConfirmedApprenticeships.ToList());
                _timer.Time("Read ApprenticeshipUpdate", () => db.ApprenticeshipUpdate.ToList());
                _timer.Time("Read AssessmentOrganisation", () => db.AssessmentOrganisation.ToList());
                _timer.Time("Read BulkUpload", () => db.BulkUpload.ToList());
                _timer.Time("Read Commitment", () => db.Commitment.ToList());
                _timer.Time("Read CustomProviderPaymentPriority", () => db.CustomProviderPaymentPriority.ToList());
                _timer.Time("Read DataLockStatus", () => db.DataLockStatus.ToList());
                _timer.Time("Read History", () => db.History.ToList());
                _timer.Time("Read IntegrationTestIds", () => db.IntegrationTestIds.ToList());
                _timer.Time("Read JobProgress", () => db.JobProgress.ToList());
                _timer.Time("Read Message", () => db.Message.ToList());
                _timer.Time("Read PriceHistory", () => db.PriceHistory.ToList());
                _timer.Time("Read TransferRequest", () => db.TransferRequest.ToList());
            }

            return Task.CompletedTask;
        }

        private ProviderDbContext CreateDbContext()
        {
            return _timer.Time("Create DB Context", () => new ProviderDbContext());
        }
    }
}
