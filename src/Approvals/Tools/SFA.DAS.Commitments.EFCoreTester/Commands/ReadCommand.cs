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
        public Task DoAsync(CancellationToken cancellationToken)
        {
            using (var db = new ProviderDbContext())
            {
                var drafts = db.DraftApprenticeships.ToList();
                var confirmed = db.ConfirmedApprenticeships.ToList();

                Console.WriteLine($"drafts.....{drafts.Count}");
                Console.WriteLine($"confirmed..{confirmed.Count}");
            }

            return Task.CompletedTask;
        }
    }
}
