using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.EFCoreTester.Interfaces
{
    public interface ICommand
    {
        Task DoAsync(CancellationToken cancellationToken);
    }
}
