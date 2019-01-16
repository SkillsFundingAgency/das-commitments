using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Commitments.EFCoreTester.Config;
using SFA.DAS.Commitments.EFCoreTester.Data;
using SFA.DAS.Commitments.EFCoreTester.Data.Models;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;

namespace SFA.DAS.Commitments.EFCoreTester.Commands
{
    public class WriteCommand : ICommand
    {
        private readonly WriteConfig _config;
        private Commitment _singleCommitment;
        private readonly ITimer _timer;

        public WriteCommand(IConfigProvider configProvider, ITimer timer)
        {
            _config = configProvider.Get<WriteConfig>();
            _timer = timer;
        }

        public async Task DoAsync(CancellationToken cancellationToken)
        {
            using (var db = CreateDbContext())
            {
                _timer.Time("Add draft apprentices", () => AddApprentices(_config.DraftCount, i => AddDraft(i, db)));
                _timer.Time("Add confirmed apprentices", () => AddApprentices(_config.ConfirmedCount, i => AddConfirmed(i, db)));

                var writes = await _timer.TimeAsync("Save changes", () => db.SaveChangesAsync(CancellationToken.None));

                Console.WriteLine($"Number of changes....{writes}");
            }
        }

        private ProviderDbContext CreateDbContext()
        {
            return _timer.Time("Create DB Context", () => new ProviderDbContext());
        }

        private void AddApprentices(int number, Action<int> action)
        {
            for (int i = 0; i < number; i++)
            {
                action(i);
            }
        }

        private void AddDraft(int i, ProviderDbContext db)
        {
            var draft = new DraftApprenticeship
            {
                //PaymentStatus = 0,
                FirstName = $"Draft.First.{i}",
                LastName = $"Draft.Last.{i}",
                Uln = $"Draft.Uln.{i}",
                PendingUpdateOriginator = 0
            };

            SetCommitment(db, draft);

            db.Add(draft);
        }

        private void AddConfirmed(int i, ProviderDbContext db)
        {
            var confirmed = new ConfirmedApprenticeship
            {
                //PaymentStatus = 1,
                AgreedOn = DateTime.UtcNow,
                FirstName = $"Confirmed.First.{i}",
                LastName = $"Confirmed.Last.{i}",
                Uln = $"Confirmed.Uln.{i}",
            };

            SetCommitment(db, confirmed);

            db.Add(confirmed);
        }

        private void SetCommitment(ProviderDbContext db, Apprenticeship apprenticeship)
        {
            Commitment commitment;

            if (_config.SingleApprenticeshipPerCommitment)
            {
                commitment = _singleCommitment ?? (_singleCommitment = CreateCommitment(db));
            }
            else
            {
                commitment = CreateCommitment(db);
            }

            apprenticeship.Commitment = commitment;
        }

        private Commitment CreateCommitment(ProviderDbContext db)
        {
            var commitment = new Commitment
            {
                Reference = $"Ref_{DateTime.UtcNow.Ticks}",
                EmployerAccountId = 1,
                LegalEntityId = "1",
                LegalEntityName = "name",
                LegalEntityAddress = "address",
                CommitmentStatus = 1,
                EditStatus = 1,
                LastAction = 1
            };

            db.Commitment.Add(commitment);

            return commitment;
        }
    }
}
