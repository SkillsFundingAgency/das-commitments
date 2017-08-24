using SFA.DAS.Commitments.Domain.Entities.DataLock;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentPayments.WebJob.Updater
{
    public interface IFilterOutAcademicYearRollOverDataLocks
    {
        Task Filter(long apprenticeshipId);
    }
}
