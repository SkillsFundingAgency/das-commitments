using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Commitments.EFCoreTester.Data;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;

namespace SFA.DAS.Commitments.EFCoreTester.Commands
{
    public class ReadCommand : ICommand
    {
        private readonly ITimer _timer;

        public ReadCommand(ITimer timer)
        {
            _timer = timer;
        }

        public Task DoAsync(CancellationToken cancellationToken)
        {
            using (var db = CreateDbContext())
            {
                var drafts = _timer.Time("Read drafts", () => db.DraftApprenticeships.ToList());
                var confirmed = _timer.Time("Read confirmed", () => db.ConfirmedApprenticeships.ToList());

                Console.WriteLine($"drafts.....{drafts.Count}");
                Console.WriteLine($"confirmed..{confirmed.Count}");
            }

            return Task.CompletedTask;
        }

        private ProviderDbContext CreateDbContext()
        {
            return _timer.Time("Create DB Context", () => new ProviderDbContext());
        }
    }
}
