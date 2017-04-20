using System.Threading.Tasks;

namespace SFA.DAS.CommitmentPayments.WebJob.Updater
{
    public interface IDataLockUpdater
    {
        Task RunUpdate();
    }
}
