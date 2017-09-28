using System.Threading.Tasks;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater
{
    public interface IAcademicYearEndExpiryProcessor
    {
        Task RunDataLock();

        Task RunApprenticeshipUpdateJob(string jobId);
    }
}