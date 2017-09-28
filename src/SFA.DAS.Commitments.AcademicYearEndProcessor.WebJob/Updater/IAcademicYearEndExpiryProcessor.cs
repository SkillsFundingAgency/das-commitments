using System.Threading.Tasks;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater
{
    public interface IAcademicYearEndExpiryProcessor
    {
        Task RunDataLock(string s);

        Task RunApprenticeshipUpdateJob(string jobId);
    }
}